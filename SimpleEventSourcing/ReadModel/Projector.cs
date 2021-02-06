using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public abstract class Projector<TState> : IProjector<TState>
        where TState : class, IEventSourcedState<TState>, new()
    {
        public TState StateModel { get; protected set; }

        protected Projector(IStateFactory stateFactory)
        {
            StateModel = stateFactory.CreateState<TState>();
        }

        protected Projector(TState stateModel)
        {
            StateModel = stateModel ?? new TState();
        }

        public abstract Task<IDisposable> StartAsync();
    }
}
