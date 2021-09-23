using SimpleEventSourcing.State;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface IProjector
    {
        Task ResetAsync();
        Task StartAsync();
    }

    public interface IProjector<TState> : IProjector
        where TState : class, IEventSourcedState<TState>, new()
    {
        TState StateModel { get; }
    }
}
