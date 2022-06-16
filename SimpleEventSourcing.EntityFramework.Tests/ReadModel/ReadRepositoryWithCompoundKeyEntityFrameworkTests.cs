using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.ReadModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    [TestFixture]
    public class ReadRepositoryWithCompoundKeyEntityFrameworkTests : ReadRepositoryWithCompoundKeyEntityFrameworkTestsBase
    {
        public ReadRepositoryWithCompoundKeyEntityFrameworkTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }
    }
}
