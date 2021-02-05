using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests
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
            await config.WriteModel.EnsureWriteDatabaseAsync();
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
            await config.WriteModel.CleanupWriteDatabaseAsync();
        }


        [Test]
        public void Can_save_stream_entry()
        {
            persistenceEngine.GetCurrentEventStoreCheckpointNumber().Should().Be(-1);

            var rawStreamEntry = config.WriteModel.GenerateRawStreamEntry();
            persistenceEngine.SaveStreamEntries(new[] { rawStreamEntry });

            persistenceEngine.GetCurrentEventStoreCheckpointNumber().Should().BeGreaterThan(-1);
        }

        [Test]
        public void Can_save_and_load_stream_entry()
        {
            var expected = config.WriteModel.GenerateRawStreamEntry();

            persistenceEngine.GetCurrentEventStoreCheckpointNumber().Should().Be(-1);

            persistenceEngine.SaveStreamEntries(new[] { expected });

            var streamEntry = persistenceEngine.LoadStreamEntries()
                .First();

            streamEntry.Payload.Should().Be(expected.Payload);
        }

        [Test]
        public void Can_save_and_load_stream_entry_by_streamname()
        {
            var expected = config.WriteModel.GenerateRawStreamEntry();

            persistenceEngine.GetCurrentEventStoreCheckpointNumber().Should().Be(-1);

            persistenceEngine.SaveStreamEntries(new[] { expected });

            var streamEntry = persistenceEngine.LoadStreamEntriesByStream(config.RawStreamEntryStreamname)
                .First();

            streamEntry.Payload.Should().Be(expected.Payload);
        }
    }
}
