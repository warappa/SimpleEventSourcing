using SimpleEventSourcing.State;

namespace SimpleEventSourcing.Domain
{
    public interface IChildEntity<out TState, TAggregateRootKey, TChildKey> : IChildEntity
        where TState : class, IStreamState, IStreamState<TState>, IChildEventSourcedState<TState, TAggregateRootKey, TChildKey>, new()
    {
        TState StateModel { get; }
    }
}
