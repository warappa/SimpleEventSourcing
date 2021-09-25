namespace SimpleEventSourcing.State
{
    public interface ISynchronousEventSourcedState<TState> : ISynchronousStateInternal<TState>
    {
    }
    public interface IAsyncEventSourcedState<TState> : IAsyncStateInternal<TState>
    {
    }
}
