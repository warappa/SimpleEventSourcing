using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Benchmarking.EFCore;
using SimpleEventSourcing.Benchmarking.NHibernate;
using SimpleEventSourcing.Benchmarking.SQLite;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking
{
    public class BenchmarkWriteModelSQLiteVsNHVsEFCore
    {
        private IPersistenceEngine sqlitePersistenceEngine;
        private IRawStreamEntryFactory sqliteRawStreamEntryFactory;
        private IPersistenceEngine efCorePersistenceEngine;
        private IRawStreamEntryFactory efCoreRawStreamEntryFactory;
        private IPersistenceEngine nhPersistenceEngine;
        private IRawStreamEntryFactory nhRawStreamEntryFactory;
        private int eventCount;

        public BenchmarkWriteModelSQLiteVsNHVsEFCore()
        {
            Setup();
        }

        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var useSystemTextJson = true;
            eventCount = 100000;

            var sqliteServiceProvider = SetupSQLite.BuildSQLite(config, useSystemTextJson);
            sqlitePersistenceEngine = sqliteServiceProvider.GetRequiredService<IPersistenceEngine>();
            sqliteRawStreamEntryFactory = sqliteServiceProvider.GetRequiredService<IRawStreamEntryFactory>();

            var efCoreServiceProvider = SetupEFCore.BuildEntityFrameworkCore(config, useSystemTextJson);
            efCorePersistenceEngine = efCoreServiceProvider.GetRequiredService<IPersistenceEngine>();
            efCoreRawStreamEntryFactory = efCoreServiceProvider.GetRequiredService<IRawStreamEntryFactory>();

            var nhServiceProvider = SetupNH.BuildNHibernate(config, useSystemTextJson);
            nhPersistenceEngine = nhServiceProvider.GetRequiredService<IPersistenceEngine>();
            nhRawStreamEntryFactory = nhServiceProvider.GetRequiredService<IRawStreamEntryFactory>();

            sqlitePersistenceEngine.InitializeAsync().Wait();
            efCorePersistenceEngine.InitializeAsync().Wait();
            nhPersistenceEngine.InitializeAsync().Wait();
        }

        [Benchmark]
        [MaxIterationCount(18)]
        public async Task BenchmarkSQLite()
        {
            //var agg = GenerateAggregateWithEvents();
            //await sqliteEventRepository.SaveAsync(agg).ConfigureAwait(false);
            var entries = GetRawStreamEntriesSQLite(Guid.NewGuid().ToString(), GetEvents(eventCount));
            await sqlitePersistenceEngine.SaveStreamEntriesAsync(entries).ConfigureAwait(false);
        }

        [Benchmark]
        [MaxIterationCount(18)]
        public async Task BenchmarkEFCore()
        {
            //var agg = GenerateAggregateWithEvents();
            //await efCoreEventRepository.SaveAsync(agg).ConfigureAwait(false);
            var entries = GetRawStreamEntriesEFCore(Guid.NewGuid().ToString(), GetEvents(eventCount));
            await efCorePersistenceEngine.SaveStreamEntriesAsync(entries).ConfigureAwait(false);
        }

        [Benchmark]
        [MaxIterationCount(18)]
        public async Task BenchmarkNH()
        {
            //var agg = GenerateAggregateWithEvents();
            //await nhEventRepository.SaveAsync(agg).ConfigureAwait(false);
            var entries = GetRawStreamEntriesNH(Guid.NewGuid().ToString(), GetEvents(eventCount));
            await nhPersistenceEngine.SaveStreamEntriesAsync(entries);
        }

        private IEvent[] GetEvents(int count)
        {
            return BenchmarkHelper.GetEvents(count);
        }

        private IRawStreamEntry[] GetRawStreamEntriesSQLite(string streamName, IEvent[] events)
        {
            return sqlitePersistenceEngine.GetRawStreamEntriesSQLite(sqliteRawStreamEntryFactory, streamName, events);
        }

        private IRawStreamEntry[] GetRawStreamEntriesEFCore(string streamName, IEvent[] events)
        {
            return efCorePersistenceEngine.GetRawStreamEntriesSQLite(efCoreRawStreamEntryFactory, streamName, events);
        }

        private IRawStreamEntry[] GetRawStreamEntriesNH(string streamName, IEvent[] events)
        {
            return nhPersistenceEngine.GetRawStreamEntriesSQLite(nhRawStreamEntryFactory, streamName, events);
        }
    }
}