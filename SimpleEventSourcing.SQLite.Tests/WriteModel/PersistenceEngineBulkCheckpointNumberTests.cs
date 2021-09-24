using NUnit.Framework;
using SimpleEventSourcing.WriteModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.WriteModel.Tests
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
            await config.ReadModel.CleanupReadDatabaseAsync();
            await config.WriteModel.CleanupWriteDatabaseAsync();
        }
    }
}
