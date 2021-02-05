using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Storage;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.Storage
{
    [TestFixture]
    public abstract class StorageResetterTests : TransactedTest
    {
        protected IStorageResetter storageResetter;
        protected Type entityTypeA;
        protected Type entityTypeB;
        protected IReadRepository readRepository;
        protected TestsBaseConfig config;

        protected StorageResetterTests(TestsBaseConfig config)
        {
            this.config = config;

            this.entityTypeA = config.ReadModel.GetTestEntityA().GetType();
            this.entityTypeB = config.ReadModel.GetTestEntityB().GetType();
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
            await config.ReadModel.CleanupReadDatabaseAsync();
            await config.WriteModel.CleanupWriteDatabaseAsync();
        }

        [Test]
        public async Task Can_create_table()
        {
            var tableExists = config.ReadModel.IsTableInDatabase(entityTypeA);

            tableExists.Should().BeFalse();

            await storageResetter.ResetAsync(new[] { entityTypeA });

            tableExists = config.ReadModel.IsTableInDatabase(entityTypeA);

            tableExists.Should().BeTrue();
        }

        [Test]
        public async Task Can_create_table_and_load_entities()
        {
            var tableExists = config.ReadModel.IsTableInDatabase(entityTypeA);

            tableExists.Should().BeFalse();

            await storageResetter.ResetAsync(new[] { entityTypeA });
            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeA, x => true)).ToList();
                res.Count.Should().Be(0);
            }

            var entity = config.ReadModel.GetTestEntityA();
            await readRepository.InsertAsync(entity).ConfigureAwait(false);

            using ((readRepository as IDbScopeAware).OpenScope())
            {
                var res = (await readRepository.QueryAsync(entityTypeA, x => true)).ToList();
                res.Count.Should().Be(1);
            }
        }
    }

    public static class ListHelper
    {
        public static IList ToList(this IQueryable query)
        {
            var genericToList = typeof(Enumerable).GetMethod("ToList")
                .MakeGenericMethod(new Type[] { query.ElementType });
            return (IList)genericToList.Invoke(null, new[] { query });
        }
    }
}
