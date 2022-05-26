using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface ISynchronousProjectorInternal<TState> : ISynchronousProjector
    {
        TState Apply(object eventOrMessage);
    }
    public interface IProjectorInternal<TState> : IAsyncProjector
    {
        Task<TState> ApplyAsync(object eventOrMessage);
    }
}
