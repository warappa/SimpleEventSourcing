using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking.Domain
{
    [Versioned("SomethingSpecialDone", 0)]
    public class SomethingSpecialDone : IEvent
    {
        public string Id { get; set; }
        public string Bla { get; set; }

        public SomethingSpecialDone(
            string id,
            string bla)
        {
            Id = id;
            Bla = bla;
        }
    }
}
