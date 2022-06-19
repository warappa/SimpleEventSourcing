using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking.Domain
{
    [Versioned("Renamed", 0)]
    public class Renamed : IEvent, INameChangeEvent
    {
        public string Id { get; set; }
        public string NewName { get; set; }

        public Renamed(
            string id,
            string newName)
        {
            Id = id;
            NewName = newName;
        }

        public string Name
        {
            get { return NewName; }
        }
    }
}