using NUnit.Framework;
using SimpleEventSourcing.Tests;

namespace SimpleEventSourcing.WriteModel.Tests
{
    [TestFixture]
    public abstract class PersistenceEngineBasicTestsBase : PersistenceEngineTestsBase
    {
        public PersistenceEngineBasicTestsBase(TestsBaseConfig config, bool initialize = true)
            : base(config, initialize)
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
