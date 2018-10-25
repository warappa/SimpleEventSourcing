using NUnit.Framework;
using SimpleEventSourcing.WriteModel.Tests;
using SQLite;
using SQLiteNetExtensions.Attributes;

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
