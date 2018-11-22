using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.Tests.Storage;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new EntityFrameworkCoreTestConfig())
        {
        }

        protected override void BeforeFixtureTransaction()
        {
            config.WriteModel.EnsureWriteDatabase();

            base.BeforeFixtureTransaction();
        }
    }
}
