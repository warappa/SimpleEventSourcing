using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public abstract class ProjectionManager<TProjector> : IProjectionManager<TProjector>, IDisposable
        where TProjector : class, IProjector, new()
    {
        private bool disposedValue;

        public TProjector Projector { get; protected set; }

        protected ProjectionManager(IStateFactory stateFactory)
        {
            Projector = stateFactory.CreateState<TProjector>();
        }

        protected ProjectionManager(TProjector projector)
        {
            Projector = projector ?? new TProjector();
        }

        public abstract Task ResetAsync();
        public abstract Task StartAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
