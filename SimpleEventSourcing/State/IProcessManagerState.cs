namespace SimpleEventSourcing.State
{
    public interface IProcessManagerState<out TState> : IEventSourcedState<TState>
        where TState : class, IStreamState<TState>, IProcessManagerState<TState>, new()
    {
        bool ProcessEnded { get; }
    }
}
