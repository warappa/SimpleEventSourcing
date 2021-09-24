using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBulkCheckpointNumberTests : PersistenceEngineBulkCheckpointNumberTestsBase
    {
        public PersistenceEngineBulkCheckpointNumberTests()
            : base(new NHibernateTestConfig())
        {
        }

        [TearDown]
        public async Task TearDownNH()
        {
            await config.ReadModel.CleanupReadDatabaseAsync();
            await config.WriteModel.CleanupWriteDatabaseAsync();
        }
    }
}
