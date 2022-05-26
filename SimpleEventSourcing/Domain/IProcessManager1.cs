using SimpleEventSourcing.State;

namespace SimpleEventSourcing.Domain
{
    public interface IProcessManager<TState, TKey> : IEventSourcedEntity<TState, TKey>, IProcessManager
        where TState : class, ISynchronousEventSourcedState<TState>, new()
    {
        TState State { get; }
    }
}
