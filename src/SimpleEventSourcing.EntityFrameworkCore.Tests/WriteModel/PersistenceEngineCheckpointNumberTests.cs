using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineCheckpointNumberTests : PersistenceEngineCheckpointNumberTestsBase
    {
        public PersistenceEngineCheckpointNumberTests()
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
