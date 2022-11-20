using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System.Collections.Generic;

namespace SimpleEventSourcing.Domain
{
    public interface IEventSourcedEntity<TState, TKey> : IEventSourcedEntity
        where TState : class, ISynchronousEventSourcedState<TState>, new()
    {
        void LoadEvents(IEnumerable<IEvent> events, TState initialState = default, int version = 0);
        new TKey Id { get; }
    }
}
