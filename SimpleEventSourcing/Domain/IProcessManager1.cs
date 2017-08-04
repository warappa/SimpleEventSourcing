using SimpleEventSourcing.State;

namespace SimpleEventSourcing.Domain
{
    public interface IProcessManager<TState, TKey> : IEventSourcedEntity<TState, TKey>, IProcessManager
        where TState : class, IEventSourcedState<TState>, new()
    {
        TState StateModel { get; }
    }
}
