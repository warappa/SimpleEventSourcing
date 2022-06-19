namespace SimpleEventSourcing.Domain
{
    public interface IChildEntityInternal
    {
        void SetAggregateRoot(IAggregateRoot aggregateRoot, object id);
    }
}
