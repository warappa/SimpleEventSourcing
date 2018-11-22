using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    [TestFixture]
    public class ReadRepositoryEntityFrameworkTests : ReadRepositoryTests
    {
        public ReadRepositoryEntityFrameworkTests()
            : base(new EntityFrameworkCoreTestConfig())
        {
        }

        protected override void BeforeFixtureTransaction()
        {
            config.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();
        }
    }
}
