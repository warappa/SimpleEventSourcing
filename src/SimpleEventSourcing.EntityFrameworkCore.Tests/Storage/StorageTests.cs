using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.Tests.Storage;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new EntityFrameworkCoreTestConfig())
        {
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.WriteModel.EnsureWriteDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }
    }
}
