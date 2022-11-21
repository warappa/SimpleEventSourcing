using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel.InMemory;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.Storage.InMemory
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new InMemoryTestConfig())
        {

        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.ReadModel.CleanupReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }
    }
}
