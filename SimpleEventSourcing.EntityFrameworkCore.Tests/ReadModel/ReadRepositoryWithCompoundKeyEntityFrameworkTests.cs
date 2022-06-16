using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    [TestFixture]
    public class ReadRepositoryWithCompoundKeyEntityFrameworkTests : ReadRepositoryWithCompoundKeyEntityFrameworkTestsBase
    {
        public ReadRepositoryWithCompoundKeyEntityFrameworkTests()
            : base(new EntityFrameworkCoreTestConfig())
        {
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }
    }
}
