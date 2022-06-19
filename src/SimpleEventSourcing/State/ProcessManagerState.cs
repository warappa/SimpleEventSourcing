using SimpleEventSourcing.Domain;

namespace SimpleEventSourcing.State
{
    public abstract class ProcessManagerState<TState> : StreamState<TState>, IProcessManagerState<TState>
        where TState : StreamState<TState>, IProcessManagerState<TState>, new()
    {
        public bool ProcessEnded { get; protected set; }

        public TState Apply(ProcessManagerHandledEvent processManagerEvent)
        {
            var newState = InvokeAssociatedApply(processManagerEvent.HandledEvent) ?? this as TState;
            (newState as ProcessManagerState<TState>).StreamName = processManagerEvent.Id;
            return newState;
        }
    }
}
