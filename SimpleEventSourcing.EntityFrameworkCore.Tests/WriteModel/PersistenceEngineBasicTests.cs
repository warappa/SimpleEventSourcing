using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new EntityFrameworkCoreTestConfig(), false)
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

            SaveStreamEntryAsync();
        }
    }
}
