using NUnit.Framework;
using SimpleEventSourcing.Tests.Storage;

namespace SimpleEventSourcing.WriteModel.InMemory.Tests
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new InMemoryTestConfig())
        {

        }

        protected override void BeforeFixtureTransaction()
        {
            config.ReadModel.CleanupReadDatabase();

            base.BeforeFixtureTransaction();
        }
    }
}
