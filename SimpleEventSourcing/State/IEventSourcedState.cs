namespace SimpleEventSourcing.State
{
    public interface IEventSourcedState<out TState> : IStateInternal<TState>
    {
    }
}
