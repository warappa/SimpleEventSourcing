using NUnit.Framework;
using SimpleEventSourcing.Tests.Storage;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.InMemory.Tests
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
            await config.ReadModel.CleanupReadDatabaseAsync();

            await base.BeforeFixtureTransactionAsync();
        }
    }
}
