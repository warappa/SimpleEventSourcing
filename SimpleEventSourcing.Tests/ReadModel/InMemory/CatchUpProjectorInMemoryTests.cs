using NUnit.Framework;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.WriteModel.InMemory.Tests;

namespace SimpleEventSourcing.ReadModel.InMemory.Tests
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
