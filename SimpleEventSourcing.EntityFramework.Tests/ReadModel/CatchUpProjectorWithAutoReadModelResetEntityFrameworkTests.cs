using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    [TestFixture]
    public class CatchUpProjectorWithAutoReadModelResetEntityFrameworkTests : CatchUpProjectorWithAutoReadModelResetTests
    {
        public CatchUpProjectorWithAutoReadModelResetEntityFrameworkTests()
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
