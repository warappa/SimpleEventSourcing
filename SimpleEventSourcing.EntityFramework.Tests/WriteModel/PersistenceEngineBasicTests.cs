using NUnit.Framework;
using SimpleEventSourcing.EntityFramework.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.EntityFramework.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new EntityFrameworkTestConfig(), false)
        {

        }

        [Test]
        public void Can_initialize()
        {
            Initialize();
        }

        [Test]
        public void Can_save_streamEntries()
        {
            Initialize();

            SaveStreamEntry();
        }
    }
}
