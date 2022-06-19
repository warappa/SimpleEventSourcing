using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking.Domain
{
    [Versioned("TestAggregateCreated", 0)]
    public class TestAggregateCreated : IEvent, INameChangeEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public TestAggregateCreated(
            string id,
            string name)
        {
            Id = id;
            Name = name;
        }

        string INameChangeEvent.Name
        {
            get { return Name; }
        }
    }
}