using NUnit.Framework;
using SimpleEventSourcing.Tests.Storage;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.Tests.Storage
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.WriteModel.EnsureWriteDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }
    }
}
