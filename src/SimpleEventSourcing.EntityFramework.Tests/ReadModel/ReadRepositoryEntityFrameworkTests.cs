using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    [TestFixture]
    public class ReadRepositoryEntityFrameworkTests : ReadRepositoryTests
    {
        public ReadRepositoryEntityFrameworkTests()
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
