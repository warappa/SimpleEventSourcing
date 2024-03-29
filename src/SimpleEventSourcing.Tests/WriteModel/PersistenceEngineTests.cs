﻿using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.WriteModel;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.WriteModel
{
    [TestFixture]
    public abstract class PersistenceEngineTests : TransactedTest
    {
        private IPersistenceEngine persistenceEngine;
        private readonly TestsBaseConfig config;

        protected PersistenceEngineTests(TestsBaseConfig config)
        {
            this.config = config;
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.WriteModel.EnsureWriteDatabaseAsync().ConfigureAwait(false);
        }

        [SetUp]
        public async Task Initialize()
        {
            persistenceEngine = config.WriteModel.GetPersistenceEngine();

            await persistenceEngine.InitializeAsync().ConfigureAwait(false);
        }

        [TearDown]
        public async Task Cleanup()
        {
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task Can_save_stream_entry()
        {
            var number = await persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            number.Should().Be(CheckpointDefaults.NoCheckpoint);

            var rawStreamEntry = config.WriteModel.GenerateRawStreamEntry();
            await persistenceEngine.SaveStreamEntriesAsync(new[] { rawStreamEntry }).ConfigureAwait(false);

            number = await persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            number.Should().BeGreaterThan(CheckpointDefaults.NoCheckpoint);
        }

        [Test]
        public async Task Can_save_and_load_stream_entry()
        {
            var expected = config.WriteModel.GenerateRawStreamEntry();

            var number = await persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            number.Should().Be(CheckpointDefaults.NoCheckpoint);

            await persistenceEngine.SaveStreamEntriesAsync(new[] { expected }).ConfigureAwait(false);
            var streamEntry = await persistenceEngine.LoadStreamEntriesAsync()
                .FirstAsync()
                .ConfigureAwait(false);
            streamEntry.Payload.Should().Be(expected.Payload);
        }

        [Test]
        public async Task Can_save_and_load_stream_entry_by_streamname()
        {
            var expected = config.WriteModel.GenerateRawStreamEntry();

            var number = await persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            number.Should().Be(CheckpointDefaults.NoCheckpoint);

            await persistenceEngine.SaveStreamEntriesAsync(new[] { expected }).ConfigureAwait(false);

            var streamEntry = await persistenceEngine.LoadStreamEntriesByStreamAsync(config.RawStreamEntryStreamname)
                .FirstAsync()
                .ConfigureAwait(false);

            streamEntry.Payload.Should().Be(expected.Payload);
        }
    }
}
