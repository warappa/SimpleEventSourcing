using NUnit.Framework;
using SimpleEventSourcing.Tests.Storage;

namespace SimpleEventSourcing.SQLite.Tests.Storage
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new SQLiteTestConfig())
        {

        }
    }
}
