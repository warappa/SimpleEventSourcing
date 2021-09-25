namespace SimpleEventSourcing.State
{
    public interface IEventSourcedState<TState> : IStateInternal<TState>
    {
    }
    public interface IAsyncEventSourcedState<TState> : IAsyncStateInternal<TState>
    {
    }
}
