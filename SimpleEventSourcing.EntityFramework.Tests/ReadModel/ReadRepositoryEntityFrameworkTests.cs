using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    [TestFixture]
    public class ReadRepositoryEntityFrameworkTests : ReadRepositoryTests
    {
        public ReadRepositoryEntityFrameworkTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        protected override void BeforeFixtureTransaction()
        {
            config.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();
        }
    }
}
