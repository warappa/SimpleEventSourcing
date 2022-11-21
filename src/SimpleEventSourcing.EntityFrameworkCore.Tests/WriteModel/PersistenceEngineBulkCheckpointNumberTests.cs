using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel
{
    [TestFixture]
    public class PersistenceEngineBulkCheckpointNumberTests : PersistenceEngineBulkCheckpointNumberTestsBase
    {
        public PersistenceEngineBulkCheckpointNumberTests()
            : base(new EntityFrameworkCoreTestConfig())
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
