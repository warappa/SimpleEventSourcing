namespace SimpleEventSourcing.State
{
    public interface ISynchronousEventSourcedState<TState> : ISynchronousProjectorInternal<TState>
    {
    }
    public interface IEventSourcedState<TState> : IProjectorInternal<TState>
    {
    }
}
