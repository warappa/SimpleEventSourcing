namespace SimpleEventSourcing.State
{
    public interface IChildEventSourcedState<TState, TAggregateRootKey, TChildKey> : IChildEventSourcedState, IStreamState<TState>
        where TState : class, IStreamState<TState>, IChildEventSourcedState<TState, TAggregateRootKey, TChildKey>, new()
    {
        new TAggregateRootKey AggregateRootId { get; }
        new TChildKey Id { get; }
    }
}
