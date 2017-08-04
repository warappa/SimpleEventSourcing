using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Domain
{
    public interface IEventSourcedEntityInternal
    {
        void RaiseEvent(IEvent @event);
    }
}
