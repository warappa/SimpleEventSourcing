using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineCheckpointNumberTests : PersistenceEngineCheckpointNumberTestsBase
    {
        public PersistenceEngineCheckpointNumberTests()
            : base(new EntityFrameworkCoreTestConfig())
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
