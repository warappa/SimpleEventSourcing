namespace SimpleEventSourcing.Messaging
{
    public interface IChildEntityEvent : IEventSourcedEntityEvent
    {
        object AggregateRootId { get; }
    }
}
