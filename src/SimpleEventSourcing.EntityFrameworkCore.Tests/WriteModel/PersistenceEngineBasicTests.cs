using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineBasicTestsBase
    {
        public PersistenceEngineBasicTests()
            : base(new EntityFrameworkCoreTestConfig(), false)
        {

        }

        [Test]
        public new async Task Can_initializeAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
        }

        [Test]
        public new async Task Can_save_streamEntriesAsync()
        {
            await InitializeAsync().ConfigureAwait(false);

            await SaveStreamEntryAsync().ConfigureAwait(false);
        }
    }
}
