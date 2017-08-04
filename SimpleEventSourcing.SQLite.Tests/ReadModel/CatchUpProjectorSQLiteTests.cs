using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.SQLite.WriteModel.Tests;

namespace SimpleEventSourcing.SQLite.ReadModel.Tests
{
    [TestFixture]
    public class CatchUpProjectorSQLiteTests : CatchUpProjectorTests
    {
        public CatchUpProjectorSQLiteTests()
            : base(new SQLiteTestConfig())
        {
        }
    }
}
