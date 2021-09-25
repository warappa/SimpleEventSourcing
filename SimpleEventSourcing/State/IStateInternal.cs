using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface IStateInternal<TState> : IState
    {
        TState Apply(object eventOrMessage);
    }
    public interface IAsyncStateInternal<TState> : IAsyncState
    {
        Task<TState> Apply(object eventOrMessage);
    }
}
