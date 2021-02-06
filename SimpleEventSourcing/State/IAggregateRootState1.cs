namespace SimpleEventSourcing.State
{
    public interface IAggregateRootState<TKey> : IAggregateRootState
    {
        new TKey Id { get; }
    }
}
