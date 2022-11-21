using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests.WriteModel;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.Storage
{
    [TestFixture]
    public abstract class StorageResetterTests : TransactedTest
    {
        protected IStorageResetter storageResetter;
        protected Type entityTypeA;
        protected Type entityTypeASubEntity;
        private readonly Type entityTypeASubItem;
        protected Type entityTypeB;
        protected IReadRepository readRepository;
        protected TestsBaseConfig config;

        protected StorageResetterTests(TestsBaseConfig config)
        {
            this.config = config;

            entityTypeA = config.ReadModel.GetTestEntityA().GetType();
            entityTypeASubEntity = config.ReadModel.GetTestEntityA().SubEntity.GetType();
            entityTypeASubItem = config.ReadModel.GetTestEntityASubItem().GetType();
            entityTypeB = config.ReadModel.GetTestEntityB().GetType();
        }

        [SetUp]
        public void TestInitialize()
        {
            storageResetter = config.ReadModel.GetStorageResetter();
            readRepository = config.ReadModel.GetReadRepository();
        }

        [TearDown]
        public async Task TearDown()
        {
            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task Can_create_table()
        {
            var tableExists = config.ReadModel.IsTableInDatabase(entityTypeA);

            tableExists.Should().BeFalse();

            await storageResetter.ResetAsync(new[] { entityTypeASubEntity, entityTypeA, entityTypeASubItem }).ConfigureAwait(false);

            tableExists = config.ReadModel.IsTableInDatabase(entityTypeA);
            tableExists.Should().BeTrue();
            tableExists = config.ReadModel.IsTableInDatabase(entityTypeASubEntity);
            tableExists.Should().BeTrue();
        }

        [Test]
        public async Task Can_create_table_and_load_entities()
        {
            var tableExists = config.ReadModel.IsTableInDatabase(entityTypeA);

            tableExists.Should().BeFalse();

            await storageResetter.ResetAsync(new[] { entityTypeASubEntity, entityTypeA, entityTypeASubItem }).ConfigureAwait(false);
            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeA, x => true).ConfigureAwait(false)).ToList();
                res.Count.Should().Be(0);
            }

            var entity = config.ReadModel.GetTestEntityA();
            await readRepository.InsertAsync(entity).ConfigureAwait(false);

            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeA, x => true).ConfigureAwait(false)).ToList();
                res.Count.Should().Be(1);
            }
        }

        [Test]
        public async Task Reset_does_not_affect_other_tables()
        {
            await storageResetter.ResetAsync(new[] { entityTypeB }).ConfigureAwait(false);

            var tableExistsB = config.ReadModel.IsTableInDatabase(entityTypeB);
            tableExistsB.Should().Be(true);

            var b = config.ReadModel.GetTestEntityB();

            await readRepository.InsertAsync(b).ConfigureAwait(false);

            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeB, x => true).ConfigureAwait(false)).ToList();
                res.Count.Should().Be(1);
            }

            var tableExists = config.ReadModel.IsTableInDatabase(entityTypeA);
            tableExists.Should().BeFalse();

            await storageResetter.ResetAsync(new[] { entityTypeASubEntity, entityTypeA, entityTypeASubItem }).ConfigureAwait(false);
            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeA, x => true).ConfigureAwait(false)).ToList();
                res.Count.Should().Be(0);
            }

            var entity = config.ReadModel.GetTestEntityA();
            await readRepository.InsertAsync(entity).ConfigureAwait(false);

            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeA, x => true).ConfigureAwait(false)).ToList();
                res.Count.Should().Be(1);
            }

            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeB, x => true).ConfigureAwait(false)).ToList();
                res.Count.Should().Be(1);
            }
        }
    }
}
