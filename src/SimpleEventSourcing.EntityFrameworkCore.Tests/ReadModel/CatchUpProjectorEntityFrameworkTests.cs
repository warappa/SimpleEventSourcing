using NUnit.Framework;
using SimpleEventSourcing.Tests.ReadModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.ReadModel
{
    [TestFixture]
    public class CatchUpProjectorEntityFrameworkTests : CatchUpProjectorTests
    {
        private EntityFrameworkCoreTestConfig efConfig => (EntityFrameworkCoreTestConfig)config;

        public CatchUpProjectorEntityFrameworkTests()
            : base(new EntityFrameworkCoreTestConfig())
        {
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await efConfig.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }
    }
}
