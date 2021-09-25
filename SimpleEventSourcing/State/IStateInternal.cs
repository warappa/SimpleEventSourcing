using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface ISynchronousStateInternal<TState> : ISynchronousState
    {
        TState Apply(object eventOrMessage);
    }
    public interface IAsyncStateInternal<TState> : IAsyncState
    {
        Task<TState> ApplyAsync(object eventOrMessage);
    }
}
