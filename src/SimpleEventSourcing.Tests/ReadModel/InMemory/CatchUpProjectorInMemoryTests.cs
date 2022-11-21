using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel.InMemory;

namespace SimpleEventSourcing.Tests.ReadModel.InMemory
{
    [TestFixture]
    public abstract class CatchUpProjectorInMemoryTests : CatchUpProjectorTests
    {
        protected CatchUpProjectorInMemoryTests()
            : base(new InMemoryTestConfig())
        {
        }
    }
}