namespace SimpleEventSourcing.State
{
    public abstract class ChildEntityState<TState, TAggregateRootKey, TChildKey> : SynchronousStreamState<TState>, IChildEventSourcedState<TState, TAggregateRootKey, TChildKey>
        where TState : ChildEntityState<TState, TAggregateRootKey, TChildKey>, IChildEventSourcedState<TState, TAggregateRootKey, TChildKey>, new()
    {
        public TChildKey Id { get; protected set; }
        public TAggregateRootKey AggregateRootId { get; protected set; }

        object IChildEventSourcedState.AggregateRootId => AggregateRootId;
        object IChildEventSourcedState.Id => Id;

        protected ChildEntityState()
        {

        }

        protected ChildEntityState(TState state)
        {
            AggregateRootId = state.AggregateRootId;
            Id = state.Id;
        }
    }
}
