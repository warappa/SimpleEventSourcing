using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.WriteModel.InMemory.Tests;

namespace SimpleEventSourcing.ReadModel.InMemory.Tests
{
    [TestFixture]
    public class CatchUpProjectorWithAutoReadModelResetInMemoryTests : CatchUpProjectorWithAutoReadModelResetTests
    {
        public CatchUpProjectorWithAutoReadModelResetInMemoryTests()
            : base(new InMemoryTestConfig())
        {
        }
    }
}
