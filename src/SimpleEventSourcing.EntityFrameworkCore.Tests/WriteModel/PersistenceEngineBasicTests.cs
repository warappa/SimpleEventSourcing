using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineBasicTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new EntityFrameworkCoreTestConfig(), false)
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
