namespace SimpleEventSourcing.Messaging
{
    public interface IEventSourcedEntityEvent<TKey> : IEventSourcedEntityEvent
    {
        new TKey Id { get; }
    }
}
