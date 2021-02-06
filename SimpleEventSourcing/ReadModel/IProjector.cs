using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface IProjector
    {
        Task<IDisposable> StartAsync();
    }

    public interface IProjector<TState>
        where TState : class, IEventSourcedState<TState>, new()
    {
        TState StateModel { get; }
        Task<IDisposable> StartAsync();
    }
}
