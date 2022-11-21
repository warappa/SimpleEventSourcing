using SimpleEventSourcing.Bus;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Newtonsoft.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.SQLite;
using SimpleEventSourcing.SQLite.ReadModel;
using SimpleEventSourcing.SQLite.Storage;
using SimpleEventSourcing.SQLite.WriteModel;
using SimpleEventSourcing.WriteModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    internal class Program
    {
        private static SQLiteConnectionWithLock writeConn;
        private static SQLiteConnectionWithLock readConn;

        private static async Task Main(string[] args)
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
                .Select(command => Observable.FromAsync(async () =>
                {
                    var aggregate = await repo.GetAsync<TestAggregate>(command.Body.Id).ConfigureAwait(false);
                    aggregate.DoSomething(command.Body.Foo);
                    await repo.SaveAsync(aggregate).ConfigureAwait(false);
                }))
                .Concat()
                .Subscribe();

            Console.WriteLine("Start executing...");

            var agg = new TestAggregate("abc" + Guid.NewGuid(), "test");

            agg.DoSomething("bla bla bla");

            agg.Rename("foo thing bar");

            agg.DoSomething("ha ha");

            agg.DoSomethingSpecial("<write poem>");

            agg.Rename("hu?");

            agg.DoSomething("hello?");

            agg.Rename("Hi!");

            await repo.SaveAsync(agg).ConfigureAwait(false);

            agg = await repo.GetAsync<TestAggregate>(agg.Id).ConfigureAwait(false);

            var projection = TestState.LoadState(agg.State);
            var projection2 = agg.State;
            Console.WriteLine("Name: " + projection.Name + ", " + projection.SomethingDone);
            Console.WriteLine("Name: " + projection2.Name + ", " + projection2.SomethingDone);

            Console.WriteLine("Try to mess with state:");

            bus.Send(new TypedMessage<TestAggregateDoSomething>(Guid.NewGuid().ToString(), new TestAggregateDoSomething { Id = agg.Id, Foo = "Command DoSomething Bla" }, null, null, null, DateTime.UtcNow, 0));
            bus.Send(new TypedMessage<TestAggregateRename>(Guid.NewGuid().ToString(), new TestAggregateRename { Id = agg.Id, Name = "Command Renamed Name" }, null, null, null, DateTime.UtcNow, 0));

            agg = await repo.GetAsync<TestAggregate>(agg.Id).ConfigureAwait(false);
            projection2 = agg.State;

            Console.WriteLine("Name: " + projection2.Name);

            // Console.WriteLine("Change-count: " + AlternateState.LoadState((AlernateState)null, agg.ev).ChangeCount);

            //SQLite3.Config(ConfigOption.Serialized);

            Console.WriteLine("-------");
            Console.WriteLine("Database Persistence");
            Console.WriteLine("-------");

            SQLiteConnectionWithLock connectionFactory()
            {
                if (writeConn != null)
                {
                    return writeConn;
                }

                //SQLite3.Config(ConfigOption.Serialized);

                var databaseFile = "writeDatabase.db";

                var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);

                writeConn = new SQLiteConnectionWithLock(connectionString);

                using (writeConn.Lock())
                {
                    writeConn.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA journal_mode = WAL;", Array.Empty<object>()).ExecuteScalar<int>();
                }

                return writeConn;
            }

            SQLiteConnectionWithLock readConnectionFactory()
            {
                if (readConn != null)
                {
                    return readConn;
                }
                //SQLite3.Config(ConfigOption.Serialized);

                var databaseFile = "readDatabase.db";

                var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);

                readConn = new SQLiteConnectionWithLock(connectionString);

                using (readConn.Lock())
                {
                    readConn.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA temp_store = MEMORY;
PRAGMA page_size = 4096;
PRAGMA cache_size = 10000;
PRAGMA journal_mode = WAL;", Array.Empty<object>()).ExecuteScalar<int>();
                }

                return readConn;
            }

            var engine = new PersistenceEngine(connectionFactory, serializer);

            engine.InitializeAsync().Wait();

            var observerFactory = new PollingObserverFactory(engine, TimeSpan.FromMilliseconds(500));
            var observer = await observerFactory.CreateObserverAsync(0).ConfigureAwait(false);
            var subscription = observer.Subscribe((s) =>
            {
                //Console.WriteLine("Polling: " + s.StreamName + "@" + s.StreamRevision + " - " + s.CheckpointNumber);
            });
            disposables.Add(subscription);
            disposables.Add(observer);

            await observer.StartAsync().ConfigureAwait(false);

            var repository = new EventRepository(
                new DefaultInstanceProvider(),
                engine,
                new RawStreamEntryFactory());

            string entityId = null;

            Console.WriteLine("Generate 1000 entities");

            var list = new List<IEventSourcedEntity>();
            for (var i = 0; i < 1000; i++)
            {
                Console.Write(".");

                entityId = Guid.NewGuid().ToString();
                var entity = new TestAggregate(entityId, "test " + DateTime.Now.ToShortTimeString());

                entity.Rename("asdfasdf");
                entity.DoSomething("bla" + DateTime.Now.ToShortTimeString());

                list.Add(entity);
            }

            await repository.SaveAsync(list).ConfigureAwait(false);

            list.Clear();

            var loadedEntity = await repository.GetAsync<TestAggregate>(entityId).ConfigureAwait(false);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var count = await engine.LoadStreamEntriesAsync().CountAsync().ConfigureAwait(false);
            stopwatch.Stop();
            Console.WriteLine($"Load {count} entries: {stopwatch.ElapsedMilliseconds}ms");
            Console.ReadKey();

            Console.WriteLine("Commits: " + await engine.LoadStreamEntriesAsync()
                .CountAsync().ConfigureAwait(false));
            Console.WriteLine("Rename count: " + await engine.LoadStreamEntriesAsync(payloadTypes: new[] { typeof(Renamed) })
                .CountAsync());

            Console.WriteLine("Rename checkpointnumbers of renames descending: " + string.Join(", ", await engine
                .LoadStreamEntriesAsync(ascending: false, payloadTypes: new[] { typeof(Renamed), typeof(SomethingDone) })
                .Select(x => "" + x.CheckpointNumber).ToArrayAsync().ConfigureAwait(false)));
            Console.WriteLine("Rename count: " + await engine.LoadStreamEntriesAsync(minCheckpointNumber: await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false)
                - 5, payloadTypes: new[] { typeof(Renamed) })
                .CountAsync().ConfigureAwait(false));
            Console.WriteLine("Current CheckpointNumber: " + await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false)
                );

            var c = readConnectionFactory();
            //c.RunInTransactionAsync((SQLiteConnection connection) =>
            //{
            c.RunInLock((SQLiteConnection connection) =>
            {
                connection.CreateTable<CheckpointInfo>(CreateFlags.AllImplicit);
            });

            var viewModelResetter = new StorageResetter(readConnectionFactory());
            var checkpointPersister = new CheckpointPersister<CheckpointInfo>(readConnectionFactory(), engine);

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

            var persistentState = new CatchUpProjectionManager<PersistentProjector>(
                new PersistentProjector(readRepository),
                checkpointPersister,
                engine,
                viewModelResetter,
                observerFactory);
            stopwatch.Start();

            await persistentState.ResetAsync().ConfigureAwait(false);
            await persistentState.StartAsync().ConfigureAwait(false);

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine(persistentState.Projector.Count);
                    await Task.Delay(1000).ConfigureAwait(false);
                }
            });

            Console.ReadKey();
            stopwatch.Stop();
            Console.WriteLine($"persistent: {persistentState.Projector.Count} msgs, {stopwatch.ElapsedMilliseconds}ms -> {persistentState.Projector.Count / (stopwatch.ElapsedMilliseconds / 1000.0)}");
            /*
            Console.ReadKey();
            Console.WriteLine(live.Projector.Name);
            Console.WriteLine(live.Projector.SomethingDone);
            Console.WriteLine(live.Projector.StreamName);


            Console.WriteLine("a: " + live2.Projector.ACount);
            Console.WriteLine("e: " + live2.Projector.ECount);
            Console.WriteLine("i: " + live2.Projector.ICount);
            Console.WriteLine("o: " + live2.Projector.OCount);
            Console.WriteLine("u: " + live2.Projector.UCount);
            */
            Console.ReadKey();

            foreach (var disp in disposables)
            {
                disp.Dispose();
            }

            observer.Dispose();
        }

        public static async Task Handle(IMessage<TestAggregateRename> command, IEventRepository repo)
        {
            var aggregate = await repo.GetAsync<TestAggregate>(command.Body.Id).ConfigureAwait(false);
            aggregate.Rename(command.Body.Name);
            await repo.SaveAsync(aggregate).ConfigureAwait(false);
        }
    }
}
