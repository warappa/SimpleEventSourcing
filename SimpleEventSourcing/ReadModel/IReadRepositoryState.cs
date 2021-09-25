using SimpleEventSourcing.State;

namespace SimpleEventSourcing.ReadModel
{
    public interface IReadRepositoryState<TState> : IAsyncEventSourcedState<TState>
        where TState : class, IAsyncEventSourcedState<TState>, new()
    {

    }
}
