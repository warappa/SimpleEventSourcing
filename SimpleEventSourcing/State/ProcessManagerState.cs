using SimpleEventSourcing.Domain;

namespace SimpleEventSourcing.State
{
    public abstract class ProcessManagerState<TState> : SynchronousStreamState<TState>, IProcessManagerState<TState>
        where TState : SynchronousStreamState<TState>, IProcessManagerState<TState>, new()
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
