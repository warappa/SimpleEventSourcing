using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.Tests.WriteModel
{
    [TestFixture]
    public class PersistenceEngineBulkCheckpointNumberTests : PersistenceEngineBulkCheckpointNumberTestsBase
    {
        public PersistenceEngineBulkCheckpointNumberTests()
            : base(new NHibernateTestConfig())
        {
        }

        [Test]
        public override Task Entities_are_in_the_same_order_as_they_were_inserted_checked_by_checkpointnumber()
        {
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            return base.Entities_are_in_the_same_order_as_they_were_inserted_checked_by_checkpointnumber();
        }

        [TearDown]
        public async Task TearDownNH()
        {
            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }
    }
}
