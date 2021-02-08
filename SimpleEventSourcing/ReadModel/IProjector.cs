using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface IProjector
    {
        Task<IDisposable> StartAsync();
    }

    public interface IProjector<TState> : IProjector
        where TState : class, IEventSourcedState<TState>, new()
    {
        TState StateModel { get; }
    }
}
