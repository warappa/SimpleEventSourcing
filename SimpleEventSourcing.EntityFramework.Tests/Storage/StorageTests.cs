using NUnit.Framework;
using SimpleEventSourcing.EntityFramework.Tests;
using SimpleEventSourcing.Tests.Storage;

namespace SimpleEventSourcing.EntityFramework.WriteModel.Tests
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new EntityFrameworkTestConfig())
        {
        }

        protected override void BeforeFixtureTransaction()
        {
            config.WriteModel.EnsureWriteDatabase();

            base.BeforeFixtureTransaction();
        }
    }
}
