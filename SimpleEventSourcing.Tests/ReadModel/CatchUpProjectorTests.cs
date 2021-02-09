using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.Tests
{
    [TestFixture]
    public abstract class CatchUpProjectorTests : TransactedTest
    {
        private CatchUpProjector<CatchUpState> target;
        protected TestsBaseConfig config;
        private ICheckpointPersister checkpointPersister;
        private IPersistenceEngine engine;
        private IStorageResetter storageResetter;
        private IPoller poller;

        protected CatchUpProjectorTests(TestsBaseConfig config)
        {
            this.config = config;

            UseTestTransaction = true;
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            engine = config.WriteModel.GetPersistenceEngine();

            await base.BeforeFixtureTransactionAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            target?.Dispose();

            await config.ReadModel.CleanupReadDatabaseAsync();
            await config.WriteModel.CleanupWriteDatabaseAsync();
        }

        [SetUp]
        public async Task Initialize()
        {
            checkpointPersister = config.ReadModel.GetCheckpointPersister();
            storageResetter = config.WriteModel.GetStorageResetter();
            poller = config.ReadModel.GetPoller(TimeSpan.FromMinutes(1));

            await engine.InitializeAsync().ConfigureAwait(false);

            target = new CatchUpProjector<CatchUpState>(null, checkpointPersister, engine, storageResetter, poller);

            var readResetter = config.ReadModel.GetStorageResetter();
            await readResetter.ResetAsync(new[] { config.ReadModel.GetTestEntityA().GetType(), config.ReadModel.GetCheckpointInfoType() });
        }

        [Test]
        public async Task Can_poll()
        {
            await target.StartAsync();

            var hasResults = await target.PollNowAsync();
            hasResults.Should().Be(false);
            target.StateModel.Count.Should().Be(0);

            await SaveRawStreamEntryAsync();

            hasResults = await target.PollNowAsync();

            hasResults.Should().Be(true);

            await Task.Delay(2000).ConfigureAwait(false);

            target.StateModel.Count.Should().Be(1);

            await SaveRawStreamEntryAsync();

            hasResults = await target.PollNowAsync();
            hasResults.Should().Be(true);

            await Task.Delay(2000).ConfigureAwait(false);
            target.StateModel.Count.Should().Be(2);
        }

        private async Task SaveRawStreamEntryAsync()
        {
            await engine.SaveStreamEntriesAsync(
                new[]
                {
                    config.WriteModel.GetRawStreamEntryFactory().CreateRawStreamEntry(
                        engine.Serializer,
                        Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString(),
                        1,
                        new TestCatchUpEvent().ToTypedMessage(Guid.NewGuid().ToString(), null, null, null, DateTime.UtcNow, 0))
                });
        }
    }

    public class TestCatchUpEvent : IEvent
    {
        public int Amount { get; set; }
    }

    public class CatchUpState : EventSourcedState<CatchUpState>
    {
        public int Count { get; set; }

        public CatchUpState Apply(TestCatchUpEvent @event)
        {
            Count++;

            return this;
        }
    }
}
