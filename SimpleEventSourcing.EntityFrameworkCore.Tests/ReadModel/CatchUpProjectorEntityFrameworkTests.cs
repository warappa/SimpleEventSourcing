using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    [TestFixture]
    public class CatchUpProjectorEntityFrameworkTests : CatchUpProjectorTests
    {
        private EntityFrameworkCoreTestConfig efConfig => (EntityFrameworkCoreTestConfig)config;

        public CatchUpProjectorEntityFrameworkTests()
            : base(new EntityFrameworkCoreTestConfig())
        {
        }

        protected override void BeforeFixtureTransaction()
        {
            efConfig.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();
        }
    }
}
