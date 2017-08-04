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

        protected CatchUpProjectorTests(TestsBaseConfig config)
        {
            this.config = config;

            UseTestTransaction = true;
        }

        protected override void BeforeFixtureTransaction()
        {
            engine = config.WriteModel.GetPersistenceEngine();

            base.BeforeFixtureTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            config.ReadModel.CleanupReadDatabase();
            config.WriteModel.CleanupWriteDatabase();
        }

        [SetUp]
        public async Task Initialize()
        {
            checkpointPersister = config.ReadModel.GetCheckpointPersister();

            storageResetter = config.WriteModel.GetStorageResetter();

            target = new CatchUpProjector<CatchUpState>(null, checkpointPersister, engine, storageResetter, 100000);

            await engine.InitializeAsync().ConfigureAwait(false);

            var readResetter = config.ReadModel.GetStorageResetter();
            readResetter.Reset(new[] { config.ReadModel.GetTestEntityA().GetType(), config.ReadModel.GetCheckpointInfoType() });
        }

        [Test]
        public async Task Can_poll()
        {
            using (var catchUp = (target.Start() as IObserveRawStreamEntries))
            {
                var hasResults = catchUp.PollNow();
                hasResults.Should().Be(false);
                target.StateModel.Count.Should().Be(0);

                SaveRawStreamEntry();

                hasResults = catchUp.PollNow();

                hasResults.Should().Be(true);

                await Task.Delay(1000).ConfigureAwait(false);

                target.StateModel.Count.Should().Be(1);

                SaveRawStreamEntry();

                hasResults = catchUp.PollNow();
                hasResults.Should().Be(true);

                await Task.Delay(2000).ConfigureAwait(false);
                target.StateModel.Count.Should().Be(2);
            }
        }

        private void SaveRawStreamEntry()
        {
            engine.SaveStreamEntries(
                                new[]{config.WriteModel.GetRawStreamEntryFactory().CreateRawStreamEntry(
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
