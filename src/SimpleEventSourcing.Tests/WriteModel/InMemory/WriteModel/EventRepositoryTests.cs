using NUnit.Framework;
using SimpleEventSourcing.WriteModel.InMemory.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.WriteModel.InMemory.WriteModel.Tests
{
    [TestFixture]
    public class EventRepositoryTests : EventRepositoryTestsBase
    {
        public EventRepositoryTests()
            : base(new InMemoryTestConfig())
        {

        }
    }
}
