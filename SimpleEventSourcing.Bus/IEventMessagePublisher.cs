using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Bus
{
    public interface IEventMessagePublisher
    {
        void Publish<T>(T @event)
            where T : IMessage<IEvent>;
    }
}
