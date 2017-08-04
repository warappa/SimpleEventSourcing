using NUnit.Framework;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.SQLite.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineBasicTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new SQLiteTestConfig(), false)
        {
        }
    }
}
