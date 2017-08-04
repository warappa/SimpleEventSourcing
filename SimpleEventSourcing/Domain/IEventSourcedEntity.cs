using SimpleEventSourcing.Messaging;
using System.Collections.Generic;

namespace SimpleEventSourcing.Domain
{
    public interface IEventSourcedEntity
    {
        void LoadEvents(IEnumerable<IEvent> events, object initialState = null);
        IEnumerable<IEvent> UncommittedEvents { get; }
        void ClearUncommittedEvents();
        int Version { get; }
        object Id { get; }
        string StreamName { get; }
        object UntypedStateModel { get; }
    }
}
