using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.Domain
{
    public abstract class ProcessManager<TState, TKey> : EventSourcedEntity<TState, TKey>, IProcessManager<TState, TKey>
        where TState : class, IStreamState<TState>, IProcessManagerState<TState>, new()
    {
        public bool ProcessEnded => State.ProcessEnded;

        protected List<ICommand> uncommittedCommands = new();

#pragma warning disable S2365 // Properties should not make collection or array copies
        IEnumerable<ICommand> IProcessManager.UncommittedCommands => uncommittedCommands.ToList();
#pragma warning restore S2365 // Properties should not make collection or array copies

        protected ProcessManager(IEnumerable<IProcessManagerHandledEvent> events)
            : base(events)
        {

        }

        void IProcessManager.Handle(IEvent @event)
        {
            ((dynamic)this).Handle((dynamic)@event);
        }

        protected void Handle(object @event)
        {
            // noop
        }

        protected void SendCommand(ICommand command)
        {
            uncommittedCommands.Add(command);
        }

        protected void HandledEvent(IEvent @event)
        {
            RaiseEvent(new ProcessManagerHandledEvent(State.ConvertToStreamName(typeof(TKey), Id), @event));
        }

        protected void HandledStartEvent(string processId, IEvent @event)
        {
            RaiseEvent(new ProcessManagerHandledEvent(processId, @event));
        }

        void IProcessManager.ClearUncommittedCommands()
        {
            uncommittedCommands.Clear();
        }
    }
}
