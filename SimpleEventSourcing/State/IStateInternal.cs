namespace SimpleEventSourcing.State
{
    public interface IStateInternal<out TState> : IState
    {
        TState Apply(object eventOrMessage);
    }
}
