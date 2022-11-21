using Shop.Core.Domain.Customers;
using Shop.ReadModel.Articles;
using Shop.ReadModel.Customers;
using Shop.ReadModel.ShoppingCarts;
using Shop.UI.Web.Shared.ReadModels.Customers;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Newtonsoft.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.SQLite;
using SimpleEventSourcing.SQLite.ReadModel;
using SimpleEventSourcing.SQLite.Storage;
using SimpleEventSourcing.SQLite.WriteModel;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Shop
{
    public partial class Program
    {
        public static ICheckpointPersister checkpointPersister;
        private static PollingObserverFactory observerFactory;
        public static IReadRepository readRepository;
        private static SQLiteConnectionWithLock writeConn;
        private static SQLiteConnectionWithLock readConn;
        private static PersistenceEngine engine;
        public static IEventRepository repository;
        public static JsonNetSerializer serializer;
        private static readonly List<IDisposable> disposeables = new();
        private static Func<SQLiteConnectionWithLock> connectionFactory;
        private static Func<SQLiteConnectionWithLock> readConnectionFactory;

        private static string GetDllPath()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase
                    .Replace("file:///", "")
                    .Replace("/", "\\");
            path = Path.GetDirectoryName(path);

            return path;
        }

        private static string GetDataPath()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase
                    .Replace("file:///", "")
                    .Replace("/", "\\");
            path = Path.GetDirectoryName(path);

            if (path.EndsWith("bin", StringComparison.Ordinal))
            {
                path = path[..^4];
            }

            return path;
        }

        private static async Task SetupWriteModelAsync()
        {
            connectionFactory = () =>
            {
                if (writeConn == null)
                {
                    var databaseFile = Path.Combine(GetDataPath(), "writeDatabase.db");

                    var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache, true, null);

                    writeConn = new SQLiteConnectionWithLock(connectionString)
                    {
                        BusyTimeout = TimeSpan.FromSeconds(2)
                    };

                    ConfigureConnection(writeConn);
                }

                return writeConn;
            };

            var binder = new VersionedBinder();

            serializer = new JsonNetSerializer(binder);
            engine = new PersistenceEngine(connectionFactory, serializer);

            await engine.InitializeAsync().ConfigureAwait(false);

            repository = new EventRepository(
                new DefaultInstanceProvider(),
                engine,
                new RawStreamEntryFactory());
        }

        private static void ConfigureConnection(SQLiteConnection conn)
        {
            var pragmas = new List<string>()
{
"PRAGMA journal_mode = WAL;",
//"PRAGMA locking_mode=EXCLUSIVE;",
"PRAGMA cache_size=20000;",
"PRAGMA page_size=32768;",
//"PRAGMA synchronous=off;"
};

            foreach (var pragma in pragmas)
            {
                conn.ExecuteScalar<string>(pragma);
            }
        }

        public static void SetupReadModel()
        {
            readConnectionFactory = () =>
            {
                if (readConn == null)
                {
                    var databaseFile = Path.Combine(GetDataPath(), "readDatabase.db");

                    var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache, true, null);

                    readConn = new SQLiteConnectionWithLock(connectionString)
                    {
                        BusyTimeout = TimeSpan.FromSeconds(2)
                    };

                    ConfigureConnection(readConn);
                }

                return readConn;
            };

            var c = readConnectionFactory();

            c.RunInLock((SQLiteConnection connection) =>
            {
                connection.CreateTable<CheckpointInfo>(CreateFlags.AllImplicit);
            });

            var viewModelResetter = new StorageResetter(readConnectionFactory());
            checkpointPersister = new CheckpointPersister<CheckpointInfo>(readConnectionFactory(), engine);
            observerFactory = new PollingObserverFactory(engine);

            // nicht-persistente Projektion
            var liveProjector = new CatchUpProjectionManager<CustomerState>(
                new CustomerState(),
                checkpointPersister,
                engine,
                viewModelResetter,
                observerFactory);

            // persistente Projektionen
            readRepository = new ReadRepository(readConnectionFactory);

            StartPersistentProjector(new CustomerReadModelState(readRepository), checkpointPersister, viewModelResetter, observerFactory);
            StartPersistentProjector(new ArticleReadModelState(readRepository), checkpointPersister, viewModelResetter, observerFactory);
            StartPersistentProjector(new ShoppingCartReadModelState(readRepository), checkpointPersister, viewModelResetter, observerFactory);

            StartPersistentProjector(new CustomerOverviewState(readRepository), checkpointPersister, viewModelResetter, observerFactory);
            StartPersistentProjector(new ArticleOverviewState(readRepository), checkpointPersister, viewModelResetter, observerFactory);
            StartPersistentProjector(new ShoppingCartOverviewState(readRepository), checkpointPersister, viewModelResetter, observerFactory);

            StartPersistentProjector(new ArticleActivationHistoryReadModelState(readRepository), checkpointPersister, viewModelResetter, observerFactory);

            disposeables.Add(liveProjector.StartAsync());
        }

        public static Task<Customer> GetOrCreateGreatCustomerAsync()
        {
            return GetOrCreateAggregateAsync<CustomerViewModel, Customer>(readRepository, x => x.Name == "Great Customer AG", () =>
            {
                var customer = new Customer(CustomerId.Generate(), "Great Customer");
                customer.Rename("Great Customer GmbH");
                customer.Rename("Great Customer AG");
                return customer;
            });
        }

        public static void StartPersistentProjector<TReadModelState>(
            TReadModelState readModelState,
            ICheckpointPersister checkpointPersister,
            IStorageResetter StorageResetter,
            IObserverFactory observerFactory)
            where TReadModelState : ReadRepositoryProjector<TReadModelState>, new()
        {
            var projection = new CatchUpProjectionManager<TReadModelState>(
                readModelState,
                checkpointPersister,
                engine,
                StorageResetter,
                observerFactory);

            disposeables.Add(projection.StartAsync());
        }

        private static async Task<TAggregate> GetOrCreateAggregateAsync<TViewModel, TAggregate>(IReadRepository readRepository, Expression<Func<TViewModel, bool>> predicate, Func<TAggregate> factory)
            where TViewModel : class, IStreamReadModel, new()
            where TAggregate : class, IAggregateRoot
        {
            TAggregate eventSourcedEntity;
            var viewModel = (await readRepository.QueryAsync(predicate).ConfigureAwait(false)).FirstOrDefault();
            if (viewModel != null)
            {
                eventSourcedEntity = await repository.GetAsync<TAggregate>(viewModel.Streamname);
            }
            else
            {
                eventSourcedEntity = factory();

                await repository.SaveAsync(eventSourcedEntity);
            }

            return eventSourcedEntity;
        }
    }
}
