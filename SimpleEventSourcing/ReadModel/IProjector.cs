using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface IProjector : IDisposable
    {
        Task ResetAsync();
        Task StartAsync();
    }

    public interface IProjector<TState> : IProjector
        where TState : class, IState, new()
    {
        TState StateModel { get; }
    }
}
