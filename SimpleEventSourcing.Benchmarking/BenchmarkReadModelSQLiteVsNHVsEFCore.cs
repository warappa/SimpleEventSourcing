using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Benchmarking.EFCore;
using SimpleEventSourcing.Benchmarking.NHibernate;
using SimpleEventSourcing.Benchmarking.ReadModel;
using SimpleEventSourcing.Benchmarking.SQLite;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking
{
    public class BenchmarkReadModelSQLiteVsNHVsEFCore
    {
        private IPersistenceEngine sqlitePersistenceEngine;
        private IRawStreamEntryFactory sqliteRawStreamEntryFactory;
        private ICheckpointPersister sqliteCheckpointPersister;
        private IServiceProvider efCoreServiceProvider;
        private IPersistenceEngine efCorePersistenceEngine;
        private IRawStreamEntryFactory efCoreRawStreamEntryFactory;
        private ICheckpointPersister efCoreCheckpointPersister;
        private IServiceProvider nhServiceProvider;
        private IPersistenceEngine nhPersistenceEngine;
        private IRawStreamEntryFactory nhRawStreamEntryFactory;
        private ICheckpointPersister nhCheckpointPersister;
        private int eventCount;
        private IServiceProvider sqliteServiceProvider;

        public BenchmarkReadModelSQLiteVsNHVsEFCore()
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

            sqliteServiceProvider = SetupSQLite.BuildSQLite(config, useSystemTextJson, true);
            sqlitePersistenceEngine = sqliteServiceProvider.GetRequiredService<IPersistenceEngine>();
            sqliteRawStreamEntryFactory = sqliteServiceProvider.GetRequiredService<IRawStreamEntryFactory>();
            sqliteCheckpointPersister = sqliteServiceProvider.GetRequiredService<ICheckpointPersister>();

            efCoreServiceProvider = SetupEFCore.BuildEntityFrameworkCore(config, useSystemTextJson, true);
            efCorePersistenceEngine = efCoreServiceProvider.GetRequiredService<IPersistenceEngine>();
            efCoreRawStreamEntryFactory = efCoreServiceProvider.GetRequiredService<IRawStreamEntryFactory>();
            efCoreCheckpointPersister = efCoreServiceProvider.GetRequiredService<ICheckpointPersister>();

            nhServiceProvider = SetupNH.BuildNHibernate(config, useSystemTextJson, true);
            nhPersistenceEngine = nhServiceProvider.GetRequiredService<IPersistenceEngine>();
            nhRawStreamEntryFactory = nhServiceProvider.GetRequiredService<IRawStreamEntryFactory>();
            nhCheckpointPersister = nhServiceProvider.GetRequiredService<ICheckpointPersister>();

            sqlitePersistenceEngine.InitializeAsync().Wait();
            efCorePersistenceEngine.InitializeAsync().Wait();
            nhPersistenceEngine.InitializeAsync().Wait();

            var sqliteEntries = GetRawStreamEntriesSQLite(Guid.NewGuid().ToString(), GetEvents(eventCount));
            sqlitePersistenceEngine.SaveStreamEntriesAsync(sqliteEntries).Wait();

            var efCoreEntries = GetRawStreamEntriesEFCore(Guid.NewGuid().ToString(), GetEvents(eventCount));
            efCorePersistenceEngine.SaveStreamEntriesAsync(efCoreEntries).Wait();

            var nhEntries = GetRawStreamEntriesNH(Guid.NewGuid().ToString(), GetEvents(eventCount));
            nhPersistenceEngine.SaveStreamEntriesAsync(nhEntries).Wait();
        }

        [Benchmark]
        [MaxIterationCount(18)]
        public async Task BenchmarkSQLite()
        {
            var cp = await sqlitePersistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);

            using var sqlitePersistentState = sqliteServiceProvider.GetRequiredService<IProjectionManager<PersistentState>>();
            await sqlitePersistentState.ResetAsync().ConfigureAwait(false);
            await sqlitePersistentState.StartAsync().ConfigureAwait(false);

            await sqliteCheckpointPersister.WaitForCheckpointNumberAsync<PersistentState>(cp, TimeSpan.FromSeconds(60)).ConfigureAwait(false);
        }

        [Benchmark]
        [MaxIterationCount(18)]
        public async Task BenchmarkEFCore()
        {
            var cp = await efCorePersistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);

            using var efCorePersistentState = efCoreServiceProvider.GetRequiredService<IProjectionManager<PersistentState>>();
            await efCorePersistentState.ResetAsync().ConfigureAwait(false);
            await efCorePersistentState.StartAsync().ConfigureAwait(false);

            await efCoreCheckpointPersister.WaitForCheckpointNumberAsync<PersistentState>(cp, TimeSpan.FromSeconds(60)).ConfigureAwait(false);
        }

        [Benchmark]
        [MaxIterationCount(18)]
        public async Task BenchmarkNH()
        {
            var cp = await nhPersistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);

            using var nhPersistentState = nhServiceProvider.GetRequiredService<IProjectionManager<PersistentState>>();
            await nhPersistentState.ResetAsync().ConfigureAwait(false);
            await nhPersistentState.StartAsync().ConfigureAwait(false);

            await nhCheckpointPersister.WaitForCheckpointNumberAsync<PersistentState>(cp, TimeSpan.FromSeconds(60)).ConfigureAwait(false);
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