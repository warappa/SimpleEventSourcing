using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Domain
{
    public abstract class AggregateRoot<TState, TKey> : EventSourcedEntity<TState, TKey>, IAggregateRoot
        where TState : class, IAggregateRootState, IStreamState<TState>, ISynchronousEventSourcedState<TState>, new()
    {
        protected AggregateRoot(IEnumerable<IEvent> events, TState initialState = null)
            : base(events, initialState)
        {
        }

        protected AggregateRoot(IEvent @event, TState initialState = null)
            : base(@event, initialState)
        {
        }

        public TChildEntity GetChildEntity<TChildEntity>(object id)
            where TChildEntity : IChildEntity, new()
        {
            var child = (TChildEntity)Activator.CreateInstance(typeof(TChildEntity));

            (child as IChildEntityInternal).SetAggregateRoot(this, id);

            return child;
        }
    }
}
