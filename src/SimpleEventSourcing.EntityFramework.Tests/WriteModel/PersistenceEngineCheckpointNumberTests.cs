using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.Tests.WriteModel
{
    [TestFixture]
    public class PersistenceEngineCheckpointNumberTests : PersistenceEngineCheckpointNumberTestsBase
    {
        public PersistenceEngineCheckpointNumberTests()
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
