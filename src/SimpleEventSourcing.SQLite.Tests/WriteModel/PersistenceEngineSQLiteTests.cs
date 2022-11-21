using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.SQLite.Tests.WriteModel
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
