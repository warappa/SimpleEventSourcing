namespace SimpleEventSourcing.Messaging
{
    public interface IEventSourcedEntityEvent : IEvent
    {
        object Id { get; }
    }
}
