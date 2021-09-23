using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public abstract class Projector<TState> : IProjector<TState>, IDisposable
        where TState : class, IEventSourcedState<TState>, new()
    {
        private bool disposedValue;

        public TState StateModel { get; protected set; }

        protected Projector(IStateFactory stateFactory)
        {
            StateModel = stateFactory.CreateState<TState>();
        }

        protected Projector(TState stateModel)
        {
            StateModel = stateModel ?? new TState();
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
