using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Benchmarking.Domain;
using SimpleEventSourcing.Benchmarking.EFCore;
using SimpleEventSourcing.Benchmarking.NHibernate;
using SimpleEventSourcing.Benchmarking.SQLite;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking
{
    public class BenchmarkSQLiteVsNHVsEFCore
    {
        private IPersistenceEngine sqlitePersistenceEngine;
        private IEventRepository sqliteEventRepository;
        private IRawStreamEntryFactory sqliteRawStreamEntryFactory;
        private IPersistenceEngine efCorePersistenceEngine;
        private IEventRepository efCoreEventRepository;
        private IRawStreamEntryFactory efCoreRawStreamEntryFactory;
        private IPersistenceEngine nhPersistenceEngine;
        private IEventRepository nhEventRepository;
        private IRawStreamEntryFactory nhRawStreamEntryFactory;

        public BenchmarkSQLiteVsNHVsEFCore()
        {
            Setup();
        }

        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var useSystemTextJson = true;

            var sqliteSericeProvider = SetupSQLite.BuildSQLite(config, useSystemTextJson);
            sqlitePersistenceEngine = sqliteSericeProvider.GetRequiredService<IPersistenceEngine>();
            sqliteEventRepository = sqliteSericeProvider.GetRequiredService<IEventRepository>();
            sqliteRawStreamEntryFactory = sqliteSericeProvider.GetRequiredService<IRawStreamEntryFactory>();

            var efCoreSericeProvider = SetupEFCore.BuildEntityFrameworkCore(config, useSystemTextJson);
            efCorePersistenceEngine = efCoreSericeProvider.GetRequiredService<IPersistenceEngine>();
            efCoreEventRepository = efCoreSericeProvider.GetRequiredService<IEventRepository>();
            efCoreRawStreamEntryFactory = efCoreSericeProvider.GetRequiredService<IRawStreamEntryFactory>();

            var nhSericeProvider = SetupNH.BuildNHibernate(config, useSystemTextJson);
            nhPersistenceEngine = nhSericeProvider.GetRequiredService<IPersistenceEngine>();
            nhEventRepository = nhSericeProvider.GetRequiredService<IEventRepository>();
            nhRawStreamEntryFactory = nhSericeProvider.GetRequiredService<IRawStreamEntryFactory>();

            sqlitePersistenceEngine.InitializeAsync().Wait();
            efCorePersistenceEngine.InitializeAsync().Wait();
            nhPersistenceEngine.InitializeAsync().Wait();
        }

        [Benchmark]
        public async Task BenchmarkSQLite()
        {
            //var agg = GenerateAggregateWithEvents();
            //await sqliteEventRepository.SaveAsync(agg);
            var entries = GetRawStreamEntriesSQLite(Guid.NewGuid().ToString(), GetEvents());
            await sqlitePersistenceEngine.SaveStreamEntriesAsync(entries);
        }

        [Benchmark]
        public async Task BenchmarkEFCore()
        {
            //var agg = GenerateAggregateWithEvents();
            //await efCoreEventRepository.SaveAsync(agg);
            var entries = GetRawStreamEntriesEFCore(Guid.NewGuid().ToString(), GetEvents());
            await efCorePersistenceEngine.SaveStreamEntriesAsync(entries);
        }

        [Benchmark]
        public async Task BenchmarkNH()
        {
            //var agg = GenerateAggregateWithEvents();
            //await nhEventRepository.SaveAsync(agg);
            var entries = GetRawStreamEntriesNH(Guid.NewGuid().ToString(), GetEvents());
            await nhPersistenceEngine.SaveStreamEntriesAsync(entries);
        }

        private IRawStreamEntry[] GetRawStreamEntriesNH(string streamName, IEvent[] events)
        {
            var commitId = Guid.NewGuid().ToString();

            var messages = events
                    .Select(@event => @event.ToTypedMessage(
                                   Guid.NewGuid().ToString(),
                                   new Dictionary<string, object>(),
                                   null,
                                   null,
                                   DateTime.UtcNow,
                                   0))
                    .ToList();

            var streamDTOs = messages
                .Select(x => nhRawStreamEntryFactory.CreateRawStreamEntry(nhPersistenceEngine.Serializer, streamName, commitId, 0, x))
                .ToArray();

            return streamDTOs;
        }

        private IRawStreamEntry[] GetRawStreamEntriesEFCore(string streamName, IEvent[] events)
        {
            var commitId = Guid.NewGuid().ToString();

            var messages = events
                    .Select(@event => @event.ToTypedMessage(
                                   Guid.NewGuid().ToString(),
                                   new Dictionary<string, object>(),
                                   null,
                                   null,
                                   DateTime.UtcNow,
                                   0))
                    .ToList();

            var streamDTOs = messages
                .Select(x => efCoreRawStreamEntryFactory.CreateRawStreamEntry(efCorePersistenceEngine.Serializer, streamName, commitId, 0, x))
                .ToArray();

            return streamDTOs;
        }

        private IRawStreamEntry[] GetRawStreamEntriesSQLite(string streamName, IEvent[] events)
        {
            var commitId = Guid.NewGuid().ToString();

            var messages = events
                    .Select(@event => @event.ToTypedMessage(
                                   Guid.NewGuid().ToString(),
                                   new Dictionary<string, object>(),
                                   null,
                                   null,
                                   DateTime.UtcNow,
                                   0))
                    .ToList();

            var streamDTOs = messages
                .Select(x => sqliteRawStreamEntryFactory.CreateRawStreamEntry(sqlitePersistenceEngine.Serializer, streamName, commitId, 0, x))
                .ToArray();

            return streamDTOs;
        }

        private static IEvent[] GetEvents()
        {
            var id = Guid.NewGuid().ToString();

            var list = new List<IEvent>();

            for (var i = 0; i < 10000; i++)
            {
                if (i == 0)
                {
                    list.Add(new TestAggregateCreated(id, "Test"));
                }
                else
                {
                    IEvent e;
                    if (i % 10 == 0)
                        e = new SomethingSpecialDone(id, "bla");
                    if (i % 10 == 1)
                        e = new SomethingDone(id, "bla");
                    if (i % 10 == 2)
                        e = new Renamed(id, "renamed");
                    if (i % 10 == 3)
                        e = new SomethingDone(id, "bla");
                    if (i % 10 == 4)
                        e = new SomethingSpecialDone(id, "bla");
                    if (i % 10 == 5)
                        e = new Renamed(id, "renamed");
                    if (i % 10 == 6)
                        e = new SomethingDone(id, "bla");
                    if (i % 10 == 7)
                        e = new Renamed(id, "renamed");
                    if (i % 10 == 8)
                        e = new SomethingDone(id, "bla");
                    //if (i % 10 == 9)
                    else
                        e = new Renamed(id, "renamed");
                    list.Add(e);
                }
            }

            return list.ToArray();
        }

        private static TestAggregate GenerateAggregateWithEvents()
        {
            var agg = new TestAggregate("abc" + Guid.NewGuid(), "test");

            agg.DoSomething("bla bla bla");

            agg.Rename("foo thing bar");

            agg.DoSomething("ha ha");

            agg.DoSomethingSpecial("<write poem>");

            agg.Rename("hu?");

            agg.DoSomething("hello?");

            agg.Rename("Hi!");

            return agg;
        }
    }
}