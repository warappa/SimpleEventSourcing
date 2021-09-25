using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface ISynchronousStateInternal<TState> : ISynchronousState
    {
        TState Apply(object eventOrMessage);
    }
    public interface IStateInternal<TState> : IAsyncState
    {
        Task<TState> ApplyAsync(object eventOrMessage);
    }
}
