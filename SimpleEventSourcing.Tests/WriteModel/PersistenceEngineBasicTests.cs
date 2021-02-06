using NUnit.Framework;
using SimpleEventSourcing.Tests;
using System.Threading.Tasks;

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
        public async Task Can_initializeAsync()
        {
            await InitializeAsync();
        }

        [Test]
        public async Task Can_save_streamEntriesAsync()
        {
            await InitializeAsync();

            await SaveStreamEntryAsync();
        }
    }
}
