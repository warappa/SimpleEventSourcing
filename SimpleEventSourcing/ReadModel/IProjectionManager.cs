using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface IProjectionManager : IDisposable
    {
        Task ResetAsync();
        Task StartAsync();
    }

    public interface IProjectionManager<TState> : IProjectionManager
        where TState : class, IProjector, new()
    {
        TState Projector { get; }
    }
}
