namespace SimpleEventSourcing.State
{
    public interface IProcessManagerState<TState> : ISynchronousEventSourcedState<TState>
        where TState : class, IStreamState<TState>, IProcessManagerState<TState>, new()
    {
        bool ProcessEnded { get; }
    }
}
