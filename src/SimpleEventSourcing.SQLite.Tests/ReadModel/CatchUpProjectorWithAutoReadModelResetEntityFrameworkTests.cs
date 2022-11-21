using NUnit.Framework;
using SimpleEventSourcing.Tests.ReadModel;
using SQLite;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.Tests.ReadModel
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
