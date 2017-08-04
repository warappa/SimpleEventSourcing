using NUnit.Framework;
using SimpleEventSourcing.EntityFramework.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.EntityFramework.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineCheckpointNumberTests : PersistenceEngineCheckpointNumberTestsBase
    {
        public PersistenceEngineCheckpointNumberTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        [TearDown]
        public void TearDownEF()
        {
            config.ReadModel.CleanupReadDatabase();
            config.WriteModel.CleanupWriteDatabase();
        }
    }
}
