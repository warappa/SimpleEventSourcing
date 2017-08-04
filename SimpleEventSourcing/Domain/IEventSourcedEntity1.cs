using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System.Collections.Generic;

namespace SimpleEventSourcing.Domain
{
    public interface IEventSourcedEntity<TState, TKey> : IEventSourcedEntity
        where TState : class, IEventSourcedState<TState>, new()
    {
        void LoadEvents(IEnumerable<IEvent> events, TState initialState = default(TState));
        new TKey Id { get; }
    }
}
