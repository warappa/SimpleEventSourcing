using SimpleEventSourcing.State;
using System;

namespace SimpleEventSourcing.ReadModel
{
    public abstract class Projector<TState> : IProjector
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

        public abstract IDisposable Start();
    }
}
