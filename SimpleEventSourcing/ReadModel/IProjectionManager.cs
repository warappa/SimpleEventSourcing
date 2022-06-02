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

    public interface IProjectionManager<TProjector> : IProjectionManager
        where TProjector : class, IProjector, new()
    {
        TProjector Projector { get; }
    }
}
