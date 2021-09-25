using SimpleEventSourcing.State;

namespace SimpleEventSourcing.ReadModel
{
    public interface IReadRepositoryState<TState> : IEventSourcedState<TState>
        where TState : class, IState, new()
    {

    }
}
