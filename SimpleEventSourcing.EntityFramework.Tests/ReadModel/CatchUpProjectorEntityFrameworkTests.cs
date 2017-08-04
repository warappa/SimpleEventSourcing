using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    [TestFixture]
    public class CatchUpProjectorEntityFrameworkTests : CatchUpProjectorTests
    {
        private EntityFrameworkTestConfig efConfig => (EntityFrameworkTestConfig)config;

        public CatchUpProjectorEntityFrameworkTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        protected override void BeforeFixtureTransaction()
        {
            efConfig.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();
        }
    }
}
