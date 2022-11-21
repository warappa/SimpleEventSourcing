using NUnit.Framework;
using SimpleEventSourcing.Tests.ReadModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.Tests.ReadModel
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
