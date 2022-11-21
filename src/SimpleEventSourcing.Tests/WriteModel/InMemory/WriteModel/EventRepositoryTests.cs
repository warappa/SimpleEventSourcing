using NUnit.Framework;

namespace SimpleEventSourcing.Tests.WriteModel.InMemory.WriteModel
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
