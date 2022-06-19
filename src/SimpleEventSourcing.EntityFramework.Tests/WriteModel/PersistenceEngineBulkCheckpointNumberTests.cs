using NUnit.Framework;
using SimpleEventSourcing.EntityFramework.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBulkCheckpointNumberTests : PersistenceEngineBulkCheckpointNumberTestsBase
    {
        public PersistenceEngineBulkCheckpointNumberTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        [TearDown]
        public async Task TearDownEF()
        {
            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }
    }
}
