﻿using EntityFramework.DbContextScope;
using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.EntityFramework.ReadModel;
using SimpleEventSourcing.EntityFramework.Storage;
using SimpleEventSourcing.EntityFramework.WriteModel;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Newtonsoft.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    public class ReadModelDbContext : DbContext, IDbContext
    {
        public ReadModelDbContext(string connectionName = null)
            : base(connectionName ?? "efRead")
        {
            Database.SetInitializer<ReadModelDbContext>(null);

            // this.Database.Log = msg => Debug.WriteLine(msg);
            Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<PersistentEntity> PersistentEntities { get; set; }
        public DbSet<CheckpointInfo> CheckpointInfos { get; set; }
    }

    public class WriteModelDbContext : DbContext, IDbContext
    {
        public WriteModelDbContext(string connectionName = null)
            : base(connectionName ?? "efWrite")
        {
            Database.SetInitializer<WriteModelDbContext>(null);

            // this.Database.Log = msg => Debug.WriteLine(msg);
        }

        public DbSet<RawStreamEntry> Commits { get; set; }
        public DbSet<RawSnapshot> Snapshots { get; set; }
    }

    public class DbContextFactory : IDbContextFactory
    {
        TDbContext IDbContextFactory.CreateDbContext<TDbContext>()
        {
            if (typeof(TDbContext) == typeof(WriteModelDbContext))
            {
                return (TDbContext)(object)new WriteModelDbContext();
            }
            else if (typeof(TDbContext) == typeof(ReadModelDbContext))
            {
                return (TDbContext)(object)new ReadModelDbContext();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    internal class ProgramEF
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Program started...");

            using (var writeDbContext = new WriteModelDbContext("efWrite"))
            {
                writeDbContext.Database.CreateIfNotExists();
            }

            using (var readDbContext = new ReadModelDbContext("efRead"))
            {
                readDbContext.Database.CreateIfNotExists();
            }

            var binder = new VersionedBinder();
            var serializer = new JsonNetSerializer(binder);

            var bus = new ObservableMessageBus();
            var dbContextScopeFactory = new DbContextScopeFactory(new DbContextFactory());
            var persistenceEngine = new PersistenceEngine<WriteModelDbContext>(dbContextScopeFactory, serializer);

            await persistenceEngine.InitializeAsync().ConfigureAwait(false);

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

            Console.WriteLine("-------");
            Console.WriteLine("Database Persistence");
            Console.WriteLine("-------");

            var engine = new PersistenceEngine<WriteModelDbContext>(dbContextScopeFactory, serializer);

            // engine.Initialize().Wait();

            var observerFactory = new PollingObserverFactory(engine, TimeSpan.FromMilliseconds(500));
            var observer = await observerFactory.CreateObserverAsync(0).ConfigureAwait(false);
            observer.Subscribe((s) =>
            {
                //Console.WriteLine("Polling: " + s.StreamName + "@" + s.StreamRevision + " - " + s.CheckpointNumber);
            });

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

            Console.WriteLine("Commits: " + await engine.LoadStreamEntriesAsync()
                .CountAsync().ConfigureAwait(false));
            Console.WriteLine("Rename count: " + await engine.LoadStreamEntriesAsync(payloadTypes: new[] { typeof(Renamed) })
                .CountAsync().ConfigureAwait(false));

            Console.WriteLine("Rename checkpointnumbers of renames descending: " + string.Join(", ", await engine
                .LoadStreamEntriesAsync(ascending: false, payloadTypes: new[] { typeof(Renamed), typeof(SomethingDone) })
                .Select(x => "" + x.CheckpointNumber).ToArrayAsync().ConfigureAwait(false)));
            Console.WriteLine("Rename count: " + await engine.LoadStreamEntriesAsync(minCheckpointNumber: await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false)
                - 5, payloadTypes: new[] { typeof(Renamed) })
                .CountAsync().ConfigureAwait(false));
            Console.WriteLine("Current CheckpointNumber: " + await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false)
                );

            var viewModelResetter = new StorageResetter<ReadModelDbContext>(dbContextScopeFactory);
            // viewModelResetter.Reset(new[] { typeof(CheckpointInfo), typeof(PersistentEntity) });
            var checkpointPersister = new CheckpointPersister<ReadModelDbContext, CheckpointInfo>(dbContextScopeFactory, persistenceEngine);

            //Console.ReadKey();

            var readRepository = new ReadRepository<ReadModelDbContext>(dbContextScopeFactory);

            //return;
            /*
            var live = new CatchUpProjector<TestState>(
                new TestState(),
                checkpointPersister,
                engine,
                viewModelResetter);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // live.Start();
            stopwatch.Stop();
            Console.WriteLine("live: " + stopwatch.ElapsedMilliseconds + "ms");

            var live2 = new CatchUpProjector<VowelCountState>(
                new VowelCountState(),
                new NullCheckpointPersister(),
                engine,
                viewModelResetter);
            // live2.Start();

            //var resetter = new StorageResetter(nHibernateResetConfigurationProvider);
            //resetter.Reset(new[] { typeof(MyPersistentEntity) });
            */
            var persistentState = new CatchUpProjectionManager<PersistentProjector>(
                new PersistentProjector(readRepository),
                checkpointPersister,
                engine,
                viewModelResetter,
                observerFactory);
            var stopwatch = new Stopwatch();
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

            observer.Dispose();
            /*
            Console.WriteLine(live.State.Name);
            Console.WriteLine(live.State.SomethingDone);
            Console.WriteLine(live.State.StreamName);
            

            Console.WriteLine("a: " + live2.State.ACount);
            Console.WriteLine("e: " + live2.State.ECount);
            Console.WriteLine("i: " + live2.State.ICount);
            Console.WriteLine("o: " + live2.State.OCount);
            Console.WriteLine("u: " + live2.State.UCount);
            */

            Console.ReadKey();
        }
    }
}
