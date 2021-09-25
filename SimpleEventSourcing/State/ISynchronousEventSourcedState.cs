namespace SimpleEventSourcing.State
{
    public interface ISynchronousEventSourcedState<TState> : ISynchronousStateInternal<TState>
    {
    }
    public interface IEventSourcedState<TState> : IStateInternal<TState>
    {
    }
}
