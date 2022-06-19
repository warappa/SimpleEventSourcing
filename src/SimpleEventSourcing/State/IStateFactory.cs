using System;

namespace SimpleEventSourcing.State
{
    public interface IStateFactory
    {
        TState CreateState<TState>();
        object CreateState(Type stateType);
    }
}
