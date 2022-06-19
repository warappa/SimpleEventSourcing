using NUnit.Framework;
using SimpleEventSourcing.EntityFramework.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineBasicTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new EntityFrameworkTestConfig(), false)
        {

        }

        [Test]
        public async Task Can_initializeAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task Can_save_streamEntriesAsync()
        {
            await InitializeAsync().ConfigureAwait(false);

            await SaveStreamEntryAsync().ConfigureAwait(false);
        }
    }
}
