using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Tests.WriteModel
{
    [Versioned("TestEvent", 0)]
    public class TestEvent : IEvent
    {
        public string Value { get; set; }
    }

    [Versioned("TestEvent2", 0)]
    public class TestEvent2 : IEvent
    {
        public string Value2 { get; set; }
    }
}
