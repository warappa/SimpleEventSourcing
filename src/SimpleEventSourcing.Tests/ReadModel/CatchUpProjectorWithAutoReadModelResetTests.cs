using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.Tests
{
    [TestFixture]
    public abstract class CatchUpProjectorWithAutoReadModelResetTests : TransactedTest
    {
        private CatchUpProjectionManager<CatchUpStateWithReadModel> target;
        protected TestsBaseConfig config;
        private IPersistenceEngine engine;
        private IReadRepository readRepository;
        private ICheckpointPersister checkpointPersister;

        protected CatchUpProjectorWithAutoReadModelResetTests(TestsBaseConfig config)
        {
            this.config = config;
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            engine = config.WriteModel.GetPersistenceEngine();

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }

        [TearDown]
        public async Task TearDown()
        {
            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }

        [SetUp]
        public async Task Initialize()
        {
            checkpointPersister = config.ReadModel.GetCheckpointPersister();
            var readStorageResetter = config.ReadModel.GetStorageResetter();
            var observerFactory = config.ReadModel.GetPollingObserverFactory(TimeSpan.FromMilliseconds(100000));

            readRepository = config.ReadModel.GetReadRepository();

            target = new CatchUpProjectionManager<CatchUpStateWithReadModel>(new CatchUpStateWithReadModel(readRepository), checkpointPersister, engine, readStorageResetter, observerFactory);

            await engine.InitializeAsync().ConfigureAwait(false);

            var readResetter = config.ReadModel.GetStorageResetter();
            await readResetter.ResetAsync(new[] { config.ReadModel.GetTestEntityA().GetType(), config.ReadModel.GetTestEntityA().SubEntity.GetType(), config.ReadModel.GetCheckpointInfoType() }).ConfigureAwait(false);
        }

        private async Task<CatchUpReadModel> Load()
        {
            using (var scope = (readRepository as IDbScopeAware).OpenScope())
            {
                return (await readRepository.QueryAsync<CatchUpReadModel>(x => true).ConfigureAwait(false)).FirstOrDefault();
            }
        }

        [Test]
        public async Task Resets_ReadModel_automatically()
        {
            await target.StartAsync().ConfigureAwait(false);

            var hasResults = await target.PollNowAsync().ConfigureAwait(false);
            hasResults.Should().Be(false);

            var model = await Load().ConfigureAwait(false);
            model.Should().Be(null);

            await SaveRawStreamEntryAsync().ConfigureAwait(false);

            hasResults = await target.PollNowAsync().ConfigureAwait(false);
            hasResults.Should().Be(true);

            var cp = await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            await checkpointPersister.WaitForCheckpointNumberAsync<CatchUpStateWithReadModel>(cp).ConfigureAwait(false);

            model = await Load().ConfigureAwait(false);
            model.Should().NotBeNull();
            model.Count.Should().Be(1);

            await SaveRawStreamEntryAsync().ConfigureAwait(false);

            hasResults = await target.PollNowAsync().ConfigureAwait(false);
            hasResults.Should().Be(true);

            cp = await engine.GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            await checkpointPersister.WaitForCheckpointNumberAsync<CatchUpStateWithReadModel>(cp).ConfigureAwait(false);

            model = await Load().ConfigureAwait(false);
            model.Count.Should().Be(2);
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
}
