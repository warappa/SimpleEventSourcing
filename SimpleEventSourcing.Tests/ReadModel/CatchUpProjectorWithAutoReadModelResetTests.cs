﻿using FluentAssertions;
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
        private CatchUpProjector<CatchUpStateWithReadModel> target;
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

            await base.BeforeFixtureTransactionAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await config.ReadModel.CleanupReadDatabaseAsync();
            await config.WriteModel.CleanupWriteDatabaseAsync();
        }

        [SetUp]
        public async Task Initialize()
        {
            checkpointPersister = config.ReadModel.GetCheckpointPersister();
            var readStorageResetter = config.ReadModel.GetStorageResetter();
            var observerFactory = config.ReadModel.GetPollingObserverFactory(TimeSpan.FromMilliseconds(100000));

            readRepository = config.ReadModel.GetReadRepository();

            target = new CatchUpProjector<CatchUpStateWithReadModel>(new CatchUpStateWithReadModel(readRepository), checkpointPersister, engine, readStorageResetter, observerFactory);

            await engine.InitializeAsync().ConfigureAwait(false);

            var readResetter = config.ReadModel.GetStorageResetter();
            await readResetter.ResetAsync(new[] { config.ReadModel.GetTestEntityA().GetType(), config.ReadModel.GetCheckpointInfoType() });
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
            await target.StartAsync();

            var hasResults = await target.PollNowAsync();
            hasResults.Should().Be(false);

            var model = await Load().ConfigureAwait(false);
            model.Should().Be(null);

            await SaveRawStreamEntryAsync();

            hasResults = await target.PollNowAsync();
            hasResults.Should().Be(true);

            var cp = await engine.GetCurrentEventStoreCheckpointNumberAsync();
            await checkpointPersister.WaitForCheckpointNumberAsync<CatchUpStateWithReadModel>(cp);
            
            model = await Load().ConfigureAwait(false);
            model.Count.Should().Be(1);

            await SaveRawStreamEntryAsync();

            hasResults = await target.PollNowAsync();
            hasResults.Should().Be(true);

            cp = await engine.GetCurrentEventStoreCheckpointNumberAsync();
            await checkpointPersister.WaitForCheckpointNumberAsync<CatchUpStateWithReadModel>(cp);

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
                });
        }
    }

    public class CatchUpReadModel : IReadModel<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        public virtual int Count { get; set; }
    }

    [ControlsReadModels(new[] { typeof(CatchUpReadModel) })]
    public class CatchUpStateWithReadModel : ReadRepositoryState<CatchUpStateWithReadModel>
    {
        private int idcounter = 0;

        public CatchUpStateWithReadModel()
            : base()
        {

        }
        public CatchUpStateWithReadModel(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public async Task<CatchUpStateWithReadModel> Apply(TestCatchUpEvent @event)
        {
            var found = false;
            await QueryAndUpdateAsync<CatchUpReadModel>(x => true, model =>
            {
                found = true;
                model.Count++;
            });

            if (!found)
            {
                await readRepository.InsertAsync(new CatchUpReadModel
                {
                    Id = ++idcounter,
                    Count = 1
                });
            }

            return this;
        }
    }
}
