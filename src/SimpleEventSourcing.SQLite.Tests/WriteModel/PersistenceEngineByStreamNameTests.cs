using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.SQLite.Tests.WriteModel
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
