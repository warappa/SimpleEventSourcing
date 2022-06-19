using NUnit.Framework;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.SQLite.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineCheckpointNumberTests : PersistenceEngineCheckpointNumberTestsBase
    {
        public PersistenceEngineCheckpointNumberTests()
             : base(new SQLiteTestConfig())
        {
        }
    }
}
