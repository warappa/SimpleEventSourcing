namespace SimpleEventSourcing.State
{
    public interface IProcessManagerState<TState> : IEventSourcedState<TState>
        where TState : class, IStreamState<TState>, IProcessManagerState<TState>, new()
    {
        bool ProcessEnded { get; }
    }
}
