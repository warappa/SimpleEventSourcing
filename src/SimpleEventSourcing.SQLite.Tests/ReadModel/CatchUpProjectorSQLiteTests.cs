using NUnit.Framework;
using SimpleEventSourcing.Tests.ReadModel;

namespace SimpleEventSourcing.SQLite.Tests.ReadModel
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
