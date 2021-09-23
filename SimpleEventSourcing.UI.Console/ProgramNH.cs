﻿// using SimpleEventSourcing.WriteModel.InMemory;

using NHibernate.Cfg;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.NHibernate.Context;
using SimpleEventSourcing.NHibernate.ReadModel;
using SimpleEventSourcing.NHibernate.Storage;
using SimpleEventSourcing.NHibernate.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    internal class ProgramNH
    {
        public static void CreateEmptyDatabase()
        {
            var cfg = GetBaseConfigurationWithoutDb();

            using (var session = cfg.BuildSessionFactory().OpenSession())
            using (var conn = session.Connection)
            {
                using var cmd = conn.CreateCommand();

                cmd.CommandText = "create database nhDatabase";

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public static Configuration GetBaseConfiguration()
        {
            var cfg = new Configuration()
                .Cache(x =>
                {
                    x.Provider<global::NHibernate.Caches.SysCache2.SysCacheProvider>();
                    x.UseQueryCache = true;
                    x.DefaultExpiration = 120;
                })
                .DataBaseIntegration(db =>
                {
                    //db.LogFormattedSql = true;
                    //db.LogSqlInConsole = true;
                    db.BatchSize = 100;
                    db.Batcher<global::NHibernate.AdoNet.SqlClientBatchingBatcherFactory>();

                    db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=nhDatabase;";
                    db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();

                })
                .SetProperty(global::NHibernate.Cfg.Environment.UseSecondLevelCache, "true")
                .SetProperty(global::NHibernate.Cfg.Environment.GenerateStatistics, "true")
                .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName)
                .SetProperty("adonet.batch_size", "100");

            return cfg;
        }

        public static Configuration GetBaseConfigurationWithoutDb()
        {
            var cfg = new Configuration()
                .DataBaseIntegration(db =>
                {
                    db.LogFormattedSql = false;
                    db.LogSqlInConsole = false;

                    db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";
                    db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();

                })
                .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

            return cfg;
        }

        private static void CreateDbIfNotExisting()
        {
            CreateEmptyDatabase();
        }

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Program started...");

            // global::HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();

            var binder = new VersionedBinder();
            var serializer = new JsonNetSerializer(binder);

            CreateDbIfNotExisting();

            var nHibernateResetConfigurationProvider = new NHibernateResetConfigurationProvider(
                GetBaseConfiguration(),
                new[]
                {
                    typeof(PersistentEntity).Assembly,
                    typeof(RawStreamEntry).Assembly,
                    typeof(CheckpointInfo).Assembly
                });



            var configuration = nHibernateResetConfigurationProvider.GetConfigurationForTypes(typeof(RawStreamEntry), typeof(CheckpointInfo), typeof(PersistentEntity));
            var sessionFactory = configuration.BuildSessionFactory();


            var bus = new ObservableMessageBus();
            var persistenceEngine = new PersistenceEngine(sessionFactory, configuration, serializer);

            await persistenceEngine.InitializeAsync();

            var repo = new EventRepository(
                new DefaultInstanceProvider(),
                persistenceEngine,
                new RawStreamEntryFactory());



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
                .Select(command => Observable.FromAsync(() => Task.Run(async () =>
                {
                    var aggregate = await repo.GetAsync<TestAggregate>(command.Body.Id);
                    aggregate.DoSomething(command.Body.Foo);
                    await repo.SaveAsync(aggregate);
                })))
                .Concat()
                .Subscribe()
                ;

            Console.WriteLine("Start executing...");

            var agg = new TestAggregate("abc" + Guid.NewGuid(), "test");

            agg.DoSomething("bla bla bla");

            agg.Rename("foo thing bar");

            agg.DoSomething("ha ha");

            agg.DoSomethingSpecial("<write poem>");

            agg.Rename("hu?");

            agg.DoSomething("hello?");

            agg.Rename("Hi!");


            await repo.SaveAsync(agg);

            agg = await repo.GetAsync<TestAggregate>(agg.Id);

            var projection = await TestState.LoadStateAsync(agg.StateModel);
            var projection2 = agg.StateModel;
            Console.WriteLine("Name: " + projection.Name + ", " + projection.SomethingDone);
            Console.WriteLine("Name: " + projection2.Name + ", " + projection2.SomethingDone);

            Console.WriteLine("Try to mess with state:");

            bus.Send(new TypedMessage<TestAggregateDoSomething>(Guid.NewGuid().ToString(), new TestAggregateDoSomething { Id = agg.Id, Foo = "Command DoSomething Bla" }, null, null, null, DateTime.UtcNow, 0));
            bus.Send(new TypedMessage<TestAggregateRename>(Guid.NewGuid().ToString(), new TestAggregateRename { Id = agg.Id, Name = "Command Renamed Name" }, null, null, null, DateTime.UtcNow, 0));

            agg = await repo.GetAsync<TestAggregate>(agg.Id);
            projection2 = agg.StateModel;

            Console.WriteLine("Name: " + projection2.Name);

            Console.WriteLine("-------");
            Console.WriteLine("Database Persistence");
            Console.WriteLine("-------");



            //var engine = new PersistenceEngine(sessionFactory, configuration, serializer);

            var observerFactory = new PollingObserverFactory(persistenceEngine, TimeSpan.FromMilliseconds(500));
            var observer = await observerFactory.CreateObserverAsync(0);
            observer.Subscribe((s) =>
            {
                //Console.WriteLine("Polling: " + s.StreamName + "@" + s.StreamRevision + " - " + s.CheckpointNumber);
            });

            await observer.StartAsync();

            var repository = new EventRepository(
                new DefaultInstanceProvider(),
                persistenceEngine,
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
            await repository.SaveAsync(list);

            list.Clear();

            var loadedEntity = await repository.GetAsync<TestAggregate>(entityId);

            Console.WriteLine("Commits: " + await persistenceEngine.LoadStreamEntriesAsync()
                .CountAsync());
            Console.WriteLine("Rename count: " + await persistenceEngine.LoadStreamEntriesAsync(payloadTypes: new[] { typeof(Renamed) })
                .CountAsync());

            Console.WriteLine("Rename checkpointnumbers of renames descending: " + string.Join(", ", await persistenceEngine
                .LoadStreamEntriesAsync(ascending: false, payloadTypes: new[] { typeof(Renamed), typeof(SomethingDone) })
                .Select(x => "" + x.CheckpointNumber).ToArrayAsync()));
            Console.WriteLine("Rename count: " + await persistenceEngine.LoadStreamEntriesAsync(
                    minCheckpointNumber: await persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync() - 5,
                    payloadTypes: new[] { typeof(Renamed) })

                .CountAsync());
            Console.WriteLine("Current CheckpointNumber: " + await persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync()
                );

            var viewModelResetter = new StorageResetter(nHibernateResetConfigurationProvider);
            var checkpointPersister = new CheckpointPersister<CheckpointInfo>(sessionFactory);

            //Console.ReadKey();

            var readRepository = new ReadRepository(sessionFactory);

            //return;
            /*
            var live = new CatchUpProjector<TestState>(
                new TestState(),
                checkpointPersister,
                persistenceEngine,
                viewModelResetter,
                2000);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            live.Start();
            stopwatch.Stop();
            Console.WriteLine("live: " + stopwatch.ElapsedMilliseconds + "ms");
            
            var live2 = new CatchUpProjector<VowelCountState>(
                new VowelCountState(),
                new NullCheckpointPersister(),
                persistenceEngine,
                viewModelResetter,
                2000);
            live2.Start();
            */
            var resetter = new StorageResetter(nHibernateResetConfigurationProvider);
            //resetter.Reset(new[] { typeof(PersistentEntity) });

            var persistentState = new CatchUpProjector<PersistentState>(
                new PersistentState(readRepository),
                checkpointPersister,
                persistenceEngine,
                viewModelResetter,
                observerFactory);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await persistentState.StartAsync();

            _ = Task.Run(async () =>
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
            observer.Dispose();
            /*
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
        }
    }
}
