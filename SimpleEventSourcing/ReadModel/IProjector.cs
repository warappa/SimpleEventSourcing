using SimpleEventSourcing.State;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface IProjector
    {
        Task ResetAsync();
        Task StartAsync();
    }

    public interface ISynchronousProjector<TState> : IProjector
        where TState : class, ISynchronousEventSourcedState<TState>, new()
    {
        TState StateModel { get; }
    }

    public interface IAsyncProjector<TState> : IProjector
        where TState : class, IAsyncEventSourcedState<TState>, new()
    {
        TState StateModel { get; }
    }
}
