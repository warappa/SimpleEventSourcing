using SimpleEventSourcing.Messaging;
using System.Collections.Generic;

namespace SimpleEventSourcing.Domain
{
    public interface IProcessManager : IEventSourcedEntity
    {
        void Handle(IEvent @event);

        IEnumerable<ICommand> UncommittedCommands { get; }
        void ClearUncommittedCommands();
    }
}
