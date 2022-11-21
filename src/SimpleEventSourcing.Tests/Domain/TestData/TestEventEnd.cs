namespace SimpleEventSourcing.Tests.Domain.TestData
{
    public class TestEventEnd : IEventWithId
    {
        public TestEventEnd(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }
    }

    public class TestEventUnknown : IEventWithId
    {
        public TestEventUnknown(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }
    }
}
