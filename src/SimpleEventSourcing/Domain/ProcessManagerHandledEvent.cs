using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Domain
{
    public class ProcessManagerHandledEvent : IProcessManagerHandledEvent
    {
        public ProcessManagerHandledEvent(string id, IEvent handledEvent)
        {
            Id = id;
            HandledEvent = handledEvent;
        }

        public IEvent HandledEvent { get; private set; }
        public string Id { get; private set; }
    }
}
