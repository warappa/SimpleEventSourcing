namespace SimpleEventSourcing.Domain
{
    public interface IAggregateRoot : IEventSourcedEntity
    {
        TChildEntity GetChildEntity<TChildEntity>(object id)
            where TChildEntity : IChildEntity, new();
    }
}
