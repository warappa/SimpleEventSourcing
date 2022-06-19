using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking.Domain
{
    [Versioned("SomethingDone", 0)]
    public class SomethingDone : IEvent
    {
        public string Id { get; set; }
        public string Bla { get; set; }

        public SomethingDone(
            string id,
            string bla)
        {
            Id = id;
            Bla = bla;
        }
    }
}