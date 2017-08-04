using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.SQLite.WriteModel.Tests;
using SQLite.Net.Interop;

namespace SimpleEventSourcing.SQLite.Tests
{
    [TestFixture]
    public class CatchUpProjectorWithAutoReadModelResetEntityFrameworkTests : CatchUpProjectorWithAutoReadModelResetTests
    {
        public CatchUpProjectorWithAutoReadModelResetEntityFrameworkTests()
            : base(new SQLiteTestConfig())
        {
        }

        protected SQLiteTestConfig SQLiteConfig => (SQLiteTestConfig)config;

        protected override void BeforeFixtureTransaction()
        {
            config.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();

            var connection = SQLiteConfig.ReadModel.GetConnection();
            var mapping = connection.GetMapping(typeof(CatchUpReadModel), CreateFlags.ImplicitPK);
        }
    }
}
