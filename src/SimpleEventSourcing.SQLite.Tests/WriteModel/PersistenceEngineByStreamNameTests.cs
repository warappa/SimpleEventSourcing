using NUnit.Framework;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.SQLite.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineByStreamNameTests : PersistenceEngineByStreamNameTestsBase
    {
        public PersistenceEngineByStreamNameTests()
             : base(new SQLiteTestConfig())
        {

        }
    }
}
