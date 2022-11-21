using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.Tests.WriteModel
{
    [TestFixture]
    public class PersistenceEngineBulkCheckpointNumberTests : PersistenceEngineBulkCheckpointNumberTestsBase
    {
        public PersistenceEngineBulkCheckpointNumberTests()
            : base(new SQLiteTestConfig())
        {
        }

        [TearDown]
        public async Task TearDownSQLite()
        {
            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);
            await config.WriteModel.CleanupWriteDatabaseAsync().ConfigureAwait(false);
        }
    }
}
