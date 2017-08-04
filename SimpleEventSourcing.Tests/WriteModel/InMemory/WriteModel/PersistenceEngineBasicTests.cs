using NUnit.Framework;
using SimpleEventSourcing.WriteModel.InMemory.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.WriteModel.InMemory.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new InMemoryTestConfig(), false)
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
