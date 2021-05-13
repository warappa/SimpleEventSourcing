using SimpleEventSourcing.Domain;
using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public abstract class ProcessManagerState<TState> : StreamState<TState>, IProcessManagerState<TState>
        where TState : StreamState<TState>, IProcessManagerState<TState>, new()
    {
        public bool ProcessEnded { get; protected set; }

        public async Task<TState> Apply(ProcessManagerHandledEvent processManagerEvent)
        {
            var newState = await InvokeAssociatedApplyAsync(processManagerEvent.HandledEvent) ?? this as TState;
            (newState as ProcessManagerState<TState>).StreamName = processManagerEvent.Id;
            return newState;
        }
    }
}
