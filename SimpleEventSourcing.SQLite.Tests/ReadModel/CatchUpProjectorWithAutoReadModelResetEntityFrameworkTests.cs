using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.SQLite.WriteModel.Tests;
using SQLite;
using System.Threading.Tasks;

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

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await config.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);

            var connection = SQLiteConfig.ReadModel.GetConnection();
            var mapping = connection.GetMapping(typeof(CatchUpReadModel), CreateFlags.ImplicitPK);
        }
    }
}
