using NUnit.Framework;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.SQLite.WriteModel.Tests;

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
