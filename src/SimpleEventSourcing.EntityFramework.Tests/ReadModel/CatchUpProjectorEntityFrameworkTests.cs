using NUnit.Framework;
using SimpleEventSourcing.Tests.ReadModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.Tests.ReadModel
{
    [TestFixture]
    public class CatchUpProjectorEntityFrameworkTests : CatchUpProjectorTests
    {
        private EntityFrameworkTestConfig efConfig => (EntityFrameworkTestConfig)config;

        public CatchUpProjectorEntityFrameworkTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await efConfig.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }
    }
}
