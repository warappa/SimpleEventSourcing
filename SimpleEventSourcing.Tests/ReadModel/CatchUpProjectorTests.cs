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
        private CatchUpProjectionManager<CatchUpState> target;
        protected TestsBaseConfig config;
        private ICheckpointPersister checkpointPersister;
        private IPersistenceEngine engine;
        private IStorageResetter storageResetter;
        private IPollingObserverFactory observerFactory;

        protected CatchUpProjectorTests(TestsBaseConfig config)
        {
            this.config = config;

            UseTestTransaction = true;
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            engine = config.WriteModel.GetPersistenceEngine();

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }

        [TearDown]
        public async Task TearDown()
        {
            target?.Dispose();

            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }

        [SetUp]
        public async Task Initialize()
        {
            checkpointPersister = config.ReadModel.GetCheckpointPersister();
            storageResetter = config.WriteModel.GetStorageResetter();
            observerFactory = config.ReadModel.GetPollingObserverFactory(TimeSpan.FromMinutes(1));

            await engine.InitializeAsync().ConfigureAwait(false);

            target = new CatchUpProjectionManager<CatchUpState>(null, checkpointPersister, engine, storageResetter, observerFactory);

            var readResetter = config.ReadModel.GetStorageResetter();
            await readResetter.ResetAsync(new[] { config.ReadModel.GetTestEntityA().GetType(), config.ReadModel.GetCheckpointInfoType() }).ConfigureAwait(false);
        }

        [Test]
        public async Task Can_poll()
        {
            await target.StartAsync().ConfigureAwait(false);

            var hasResults = await target.PollNowAsync().ConfigureAwait(false);
            hasResults.Should().Be(false);
            target.Projector.Count.Should().Be(0);

            await SaveRawStreamEntryAsync().ConfigureAwait(false);

            hasResults = await target.PollNowAsync().ConfigureAwait(false);

            hasResults.Should().Be(true);

            var cp = await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            await checkpointPersister.WaitForCheckpointNumberAsync<CatchUpState>(cp).ConfigureAwait(false);

            target.Projector.Count.Should().Be(1);

            await SaveRawStreamEntryAsync().ConfigureAwait(false);

            hasResults = await target.PollNowAsync().ConfigureAwait(false);
            hasResults.Should().Be(true);

            cp = await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            await checkpointPersister.WaitForCheckpointNumberAsync<CatchUpState>(cp).ConfigureAwait(false);

            target.Projector.Count.Should().Be(2);
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
                })
                .ConfigureAwait(false);
        }
    }

    public class TestCatchUpEvent : IEvent
    {
        public int Amount { get; set; }
    }

    public class CatchUpState : AsyncEventSourcedProjector<CatchUpState>
    {
        public int Count { get; set; }

        public CatchUpState Apply(TestCatchUpEvent @event)
        {
            Count++;

            return this;
        }
    }
}
