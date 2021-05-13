using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface IStateInternal<TState> : IState
    {
        Task<TState> Apply(object eventOrMessage);
    }
}
