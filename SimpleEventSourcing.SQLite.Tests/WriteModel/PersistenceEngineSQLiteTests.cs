using NUnit.Framework;
using SimpleEventSourcing.SQLite.WriteModel.Tests;
using SimpleEventSourcing.Tests;

namespace SimpleEventSourcing.SQLite.Tests
{
    [TestFixture]
    public class PersistenceEngineSQLiteTests : PersistenceEngineTests
    {
        public PersistenceEngineSQLiteTests()
            : base(new SQLiteTestConfig())
        {

        }
    }
}
