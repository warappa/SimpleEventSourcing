using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Domain
{
    public interface IProcessManagerStartsWith<TEvent> : IProcessManagerStartsWith
        where TEvent : IEvent
    {
        void Handle(string processId, TEvent startEvent);
    }
}
