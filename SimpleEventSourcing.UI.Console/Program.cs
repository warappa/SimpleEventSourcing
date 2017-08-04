using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Diagnostics;
using SQLite.Net;
using SQLite.Net.Interop;
using SimpleEventSourcing.WriteModel;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.SQLite.ReadModel;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.SQLite.WriteModel;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.SQLite.Storage;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    class Program
    {
        private static SQLiteConnectionWithLock writeConn;
        private static SQLiteConnectionWithLock readConn;
        static void Main(string[] args)
        {
            var disposables = new List<IDisposable>();

            Console.WriteLine("Program started...");

            var binder = new VersionedBinder();
            var serializer = new JsonNetSerializer(binder);

            var bus = new ObservableMessageBus();
            var repo = new EventRepository(new DefaultInstanceProvider(), new WriteModel.InMemory.PersistenceEngine(serializer), new WriteModel.InMemory.RawStreamEntryFactory());

            // let bus subscribe to repository and publish its committed events
            repo.SubscribeTo<IMessage<IEvent>>()
                .Subscribe((e) => bus.Publish(e));

            // normal event processing
            bus.SubscribeTo<IMessage<IEvent>>().Subscribe((e) =>
                {
                    Console.WriteLine("Look, an event: " + e.ToString());
                });

            bus.SubscribeTo<SomethingDone>()
                .Subscribe((e) =>
                {
                    Console.WriteLine("Something done: " + e.Bla);
                });
            bus.SubscribeTo<Renamed>()
                .Subscribe((e) =>
                {
                    Console.WriteLine("Renamed: " + e.Name);
                });

            // complex event processing
            bus.SubscribeTo<IEvent>()
                .Buffer(2, 1)
                .Where(x =>
                    x.First() is SomethingDone &&
                    x.Last() is INameChangeEvent)
                .Subscribe((e) =>
            {
                Console.WriteLine("IMessage<IEvent>: Look, a new name after something done: " + (e.First() as SomethingDone).Bla + " -> " + (e.Last() as INameChangeEvent).Name);
            });

            bus.SubscribeTo<IEvent>()
                .Buffer(2, 1)
                .Where(x =>
                    x.First() is SomethingDone &&
                    x.Last() is INameChangeEvent)
                .Subscribe((e) =>
                {
                    Console.WriteLine("IEvent: Look, a new name after something done: " + (e.First() as SomethingDone).Bla + " -> " + (e.Last() as INameChangeEvent).Name);
                });

            bus
                .SubscribeToAndUpdate<IMessage<TestAggregateRename>, TestAggregate>((aggr, cmd) =>
                {
                    aggr.Rename(cmd.Body.Name);
                }, repo);

            bus.SubscribeTo<IMessage<TestAggregateDoSomething>>()
                .Subscribe(
                command =>
                {
                    var aggregate = repo.Get<TestAggregate>(command.Body.Id);
                    aggregate.DoSomething(command.Body.Foo);
                    repo.Save(aggregate);
                });

            Console.WriteLine("Start executing...");

            var agg = new TestAggregate("abc" + Guid.NewGuid(), "test");

            agg.DoSomething("bla bla bla");

            agg.Rename("foo thing bar");

            agg.DoSomething("ha ha");

            agg.DoSomethingSpecial("<write poem>");

            agg.Rename("hu?");

            agg.DoSomething("hello?");

            agg.Rename("Hi!");


            repo.Save(agg);

            agg = repo.Get<TestAggregate>(agg.Id);

            var projection = TestState.LoadState(agg.StateModel);
            var projection2 = agg.StateModel;
            Console.WriteLine("Name: " + projection.Name + ", " + projection.SomethingDone);
            Console.WriteLine("Name: " + projection2.Name + ", " + projection2.SomethingDone);

            Console.WriteLine("Try to mess with state:");

            bus.Send(new TypedMessage<TestAggregateDoSomething>(Guid.NewGuid().ToString(), new TestAggregateDoSomething { Id = agg.Id, Foo = "Command DoSomething Bla" }, null, null, null, DateTime.UtcNow, 0));
            bus.Send(new TypedMessage<TestAggregateRename>(Guid.NewGuid().ToString(), new TestAggregateRename { Id = agg.Id, Name = "Command Renamed Name" }, null, null, null, DateTime.UtcNow, 0));

            agg = repo.Get<TestAggregate>(agg.Id);
            projection2 = agg.StateModel;

            Console.WriteLine("Name: " + projection2.Name);

            // Console.WriteLine("Change-count: " + AlternateState.LoadState((AlernateState)null, agg.ev).ChangeCount);


            //SQLite3.Config(ConfigOption.Serialized);

            Console.WriteLine("-------");
            Console.WriteLine("Database Persistence");
            Console.WriteLine("-------");

            Func<SQLiteConnectionWithLock> connectionFactory = () =>
            {
                if (writeConn != null)
                {
                    return writeConn;
                }

                //SQLite3.Config(ConfigOption.Serialized);

                var databaseFile = "writeDatabase.db";

                var connectionString = new SQLiteConnectionString(databaseFile, true, null, null, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

                writeConn = new SQLiteConnectionWithLock(new global::SQLite.Net.Platform.Win32.SQLitePlatformWin32(), connectionString);

                using (writeConn.Lock())
                {
                    writeConn.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA journal_mode = WAL;", new object[0]).ExecuteScalar<int>();
                }

                return writeConn;
            };

            Func<SQLiteConnectionWithLock> readConnectionFactory = () =>
            {
                if (readConn != null)
                {
                    return readConn;
                }
                //SQLite3.Config(ConfigOption.Serialized);

                var databaseFile = "readDatabase.db";

                var connectionString = new SQLiteConnectionString(databaseFile, true, null, null, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

                readConn = new SQLiteConnectionWithLock(new global::SQLite.Net.Platform.Win32.SQLitePlatformWin32(), connectionString);

                using (readConn.Lock())
                {
                    readConn.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA temp_store = MEMORY;
PRAGMA page_size = 4096;
PRAGMA cache_size = 10000;
PRAGMA journal_mode = WAL;", new object[0]).ExecuteScalar<int>();
                }

                return readConn;
            };

            var engine = new PersistenceEngine(connectionFactory, serializer);

            engine.InitializeAsync().Wait();

            var polling = new Poller(engine, 500);
            var observer = polling.ObserveFrom(0);
            observer.Subscribe((s) =>
            {
                //Console.WriteLine("Polling: " + s.StreamName + "@" + s.StreamRevision + " - " + s.CheckpointNumber);
            });
            disposables.Add(observer);
            var completionTask = observer.StartAsync();

            var repository = new EventRepository(
                new DefaultInstanceProvider(),
                engine,
                new RawStreamEntryFactory());

            string entityId = null;

            Console.WriteLine("Generate 1000 entities");

            var list = new List<IEventSourcedEntity>();
            for (var i = 0; i < 1; i++)
            {
                Console.Write(".");

                entityId = Guid.NewGuid().ToString();
                var entity = new TestAggregate(entityId, "test " + DateTime.Now.ToShortTimeString());

                entity.Rename("asdfasdf");
                entity.DoSomething("bla" + DateTime.Now.ToShortTimeString());

                list.Add(entity);
            }
            repository.Save(list);

            list.Clear();


            var loadedEntity = repository.Get<TestAggregate>(entityId);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var count = engine.LoadStreamEntries().Count();
            stopwatch.Stop();
            Console.WriteLine($"Load {count} entries: {stopwatch.ElapsedMilliseconds}ms");
            Console.ReadKey();

            Console.WriteLine("Commits: " + engine.LoadStreamEntries()
                //.Result
                .Count());
            Console.WriteLine("Rename count: " + engine.LoadStreamEntries(payloadTypes: new[] { typeof(Renamed) })
                //.Result
                .Count());

            Console.WriteLine("Rename checkpointnumbers of renames descending: " + string.Join(", ", engine
                .LoadStreamEntries(ascending: false, payloadTypes: new[] { typeof(Renamed), typeof(SomethingDone) })
                //.Result
                .Select(x => "" + x.CheckpointNumber).ToArray()));
            Console.WriteLine("Rename count: " + engine.LoadStreamEntries(minCheckpointNumber: engine.GetCurrentEventStoreCheckpointNumber()
                //.Result
                - 5, payloadTypes: new[] { typeof(Renamed) })
                //.Result
                .Count());
            Console.WriteLine("Current CheckpointNumber: " + engine.GetCurrentEventStoreCheckpointNumber()
                //.Result
                );

            var c = readConnectionFactory();
            //c.RunInTransactionAsync((SQLiteConnection connection) =>
            //{
            c.RunInLock((SQLiteConnection connection) =>
            {
                connection.CreateTable<CheckpointInfo>(CreateFlags.AllImplicit);
            });

            var viewModelResetter = new StorageResetter(readConnectionFactory());
            var checkpointPersister = new CheckpointPersister<CheckpointInfo>(readConnectionFactory());

            //Console.ReadKey();

            var readRepository = new ReadRepository(readConnectionFactory);

            //return;
            /*
            var live = new CatchUpProjector<TestState>(
                new TestState(),
                new NullCheckpointPersister(),
                engine,
                viewModelResetter);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            live.Start();
            stopwatch.Stop();
            Console.WriteLine("live: " + stopwatch.ElapsedMilliseconds + "ms");

            var live2 = new CatchUpProjector<VowelCountState>(
                new VowelCountState(),
                new NullCheckpointPersister(),
                engine,
                viewModelResetter);
            live2.Start();

            var resetter = new StorageResetter(readConnectionFactory());
            //resetter.Reset(new[] { typeof(PersistentEntity) });
            */

            var persistentState = new CatchUpProjector<PersistentState>(
                new PersistentState(readRepository),
                checkpointPersister,
                engine,
                viewModelResetter);
            stopwatch.Start();

            disposables.Add(persistentState.Start());

            Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine(persistentState.StateModel.Count);
                    await Task.Delay(1000).ConfigureAwait(false);
                }
            });

            Console.ReadKey();
            stopwatch.Stop();
            Console.WriteLine($"persistent: {persistentState.StateModel.Count} msgs, {stopwatch.ElapsedMilliseconds}ms -> {persistentState.StateModel.Count / (stopwatch.ElapsedMilliseconds / 1000.0)}");
            /*
            Console.ReadKey();
            Console.WriteLine(live.StateModel.Name);
            Console.WriteLine(live.StateModel.SomethingDone);
            Console.WriteLine(live.StateModel.StreamName);


            Console.WriteLine("a: " + live2.StateModel.ACount);
            Console.WriteLine("e: " + live2.StateModel.ECount);
            Console.WriteLine("i: " + live2.StateModel.ICount);
            Console.WriteLine("o: " + live2.StateModel.OCount);
            Console.WriteLine("u: " + live2.StateModel.UCount);
            */
            Console.ReadKey();

            foreach (var disp in disposables)
            {
                disp.Dispose();
            }
            completionTask.Wait();
        }

        public static void Handle(IMessage<TestAggregateRename> command, IEventRepository repo)
        {
            var aggregate = repo.Get<TestAggregate>(command.Body.Id);
            aggregate.Rename(command.Body.Name);
            repo.Save(aggregate);
        }
    }
}
