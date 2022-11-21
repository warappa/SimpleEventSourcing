using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel.InMemory;

namespace SimpleEventSourcing.Tests.ReadModel.InMemory
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
