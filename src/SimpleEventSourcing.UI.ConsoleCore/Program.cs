using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EfCoreCheckpointInfo = SimpleEventSourcing.EntityFrameworkCore.ReadModel.CheckpointInfo;
using NHCheckpointInfo = SimpleEventSourcing.NHibernate.ReadModel.CheckpointInfo;
using SQLiteCheckpointInfo = SimpleEventSourcing.SQLite.ReadModel.CheckpointInfo;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    internal class Program
    {
        public static async Task Run(Func<IConfigurationRoot, IServiceProvider> serviceProviderFactory)
        {
            Console.WriteLine("Program started...");

            var configuration = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json")
                  .Build();

            var serviceProvider = serviceProviderFactory(configuration);

            var bus = serviceProvider.GetRequiredService<IObservableMessageBus>();
            var persistenceEngine = serviceProvider.GetRequiredService<IPersistenceEngine>();
            var repo = serviceProvider.GetRequiredService<IEventRepository>();
            var repository = serviceProvider.GetRequiredService<IEventRepository>();
            var checkpointPersister = serviceProvider.GetRequiredService<ICheckpointPersister>();
            var observerFactory = serviceProvider.GetRequiredService<IObserverFactory>();
            var readRepository = serviceProvider.GetRequiredService<IReadRepository>();
            var persistentState = serviceProvider.GetRequiredService<IProjectionManager<PersistentState>>();
            var viewModelResetter = serviceProvider.GetRequiredService<IReadModelStorageResetter>();
            var rawStreamEntryFactory = serviceProvider.GetRequiredService<IRawStreamEntryFactory>();

            await persistenceEngine.InitializeAsync().ConfigureAwait(false);

            // let bus subscribe to repository and publish its committed events
            repo.SubscribeTo<IMessage<IEvent>>()
                .Subscribe((e) => bus.Publish(e));

            // normal event processing
            bus.SubscribeTo<IMessage<IEvent>>().Subscribe((e) =>
            {
                // Console.WriteLine("Look, an event: " + e.ToString());
            });

            bus.SubscribeTo<SomethingDone>()
                .Subscribe((e) =>
                {
                    //Console.WriteLine("Something done: " + e.Bla);
                });
            bus.SubscribeTo<Renamed>()
                .Subscribe((e) =>
                {
                    //Console.WriteLine("Renamed: " + e.Name);
                });

            // complex event processing
            bus.SubscribeTo<IEvent>()
                .Buffer(2, 1)
                .Where(x =>
                    x.First() is SomethingDone &&
                    x.Last() is INameChangeEvent)
                .Subscribe((e) =>
                {
                    //Console.WriteLine("IMessage<IEvent>: Look, a new name after something done: " + (e.First() as SomethingDone).Bla + " -> " + (e.Last() as INameChangeEvent).Name);
                });

            bus.SubscribeTo<IEvent>()
                .Buffer(2, 1)
                .Where(x =>
                    x.First() is SomethingDone &&
                    x.Last() is INameChangeEvent)
                .Subscribe((e) =>
                {
                    //Console.WriteLine("IEvent: Look, a new name after something done: " + (e.First() as SomethingDone).Bla + " -> " + (e.Last() as INameChangeEvent).Name);
                });

            bus
                .SubscribeToAndUpdate<IMessage<TestAggregateRename>, TestAggregate>((aggr, cmd) =>
                {
                    //aggr.Rename(cmd.Body.Name);
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

            var engine = persistenceEngine;

            var observer = await observerFactory.CreateObserverAsync(0).ConfigureAwait(false);
            observer.Subscribe((s) =>
            {
                //Console.WriteLine("Polling: " + s.StreamName + "@" + s.StreamRevision + " - " + s.CheckpointNumber);
            });

            await observer.StartAsync().ConfigureAwait(false);

            string entityId = null;

            Console.WriteLine("Generate 1000 entities");

            var list = new List<IEventSourcedEntity>();
            for (var i = 0; i < 10000; i++)
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

            await InsertEventThatIsNotHandledInPersistentState(persistenceEngine, rawStreamEntryFactory);

            var loadedEntity = await repository.GetAsync<TestAggregate>(entityId).ConfigureAwait(false);

            //Console.WriteLine("Commits: " + await engine.LoadStreamEntriesAsync().CountAsync());
            //Console.WriteLine("Rename count: " + await engine.LoadStreamEntriesAsync(payloadTypes: new[] { typeof(Renamed) })
            //    .CountAsync());

            //Console.WriteLine("Rename checkpointnumbers of renames descending: " + string.Join(", ", await engine
            //    .LoadStreamEntriesAsync(ascending: false, payloadTypes: new[] { typeof(Renamed), typeof(SomethingDone) })
            //    .Select(x => "" + x.CheckpointNumber)
            //    .ToListAsync()));
            //Console.WriteLine("Rename count: " + await engine.LoadStreamEntriesAsync(minCheckpointNumber: await engine.GetCurrentEventStoreCheckpointNumberAsync()
            //    - 5, payloadTypes: new[] { typeof(Renamed) })
            //    .CountAsync());
            //Console.WriteLine("Current CheckpointNumber: " + await engine.GetCurrentEventStoreCheckpointNumberAsync());

            var checkpointInfotype = viewModelResetter.GetType().Namespace.Contains("EntityFrameworkCore") ?
                typeof(EfCoreCheckpointInfo) : viewModelResetter.GetType().Namespace.Contains("NHibernate") ?
                typeof(NHCheckpointInfo) :
                typeof(SQLiteCheckpointInfo);

            await viewModelResetter.ResetAsync(new[] { checkpointInfotype, typeof(PersistentEntity) }).ConfigureAwait(false);

            WaitForInput();

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

            var initialCheckpointNumber = await checkpointPersister.LoadLastCheckpointAsync(nameof(PersistentState)).ConfigureAwait(false);

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

            var endCP = await persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            while (true)
            {
                var cp = await checkpointPersister.LoadLastCheckpointAsync(nameof(PersistentState)).ConfigureAwait(false);

                if (cp == endCP)
                {
                    break;
                }

                await Task.Delay(333).ConfigureAwait(false);
            }

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

            WaitForInput();
        }

        private static async Task InsertEventThatIsNotHandledInPersistentState(IPersistenceEngine persistenceEngine, IRawStreamEntryFactory rawStreamEntryFactory)
        {
            var notHandledEvent = new EventNotHandledByPersistentState(Guid.NewGuid().ToString(), true);
            var notHandledMessage = notHandledEvent.ToTypedMessage(Guid.NewGuid().ToString(), new Dictionary<string, object>(), null, null, DateTime.UtcNow, 0);
            var rawStreamEntry = rawStreamEntryFactory.CreateRawStreamEntry(persistenceEngine.Serializer, notHandledEvent.Id, Guid.NewGuid().ToString(), 0, notHandledMessage);
            await persistenceEngine.SaveStreamEntriesAsync(new[] { rawStreamEntry });
        }

        private static void WaitForInput()
        {
            Console.WriteLine("\n\nPress any key to continue");
            Console.ReadKey();
        }
    }
}
