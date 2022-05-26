using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.Domain
{
    public abstract class ChildEntity<TState, TAggregateRootkey, TChildKey> : IChildEntity<TState, TAggregateRootkey, TChildKey>, IChildEntityInternal
        where TState : class, IStreamState<TState>, IChildEventSourcedState<TState, TAggregateRootkey, TChildKey>, new()
    {
        public TChildKey Id { get; protected set; }
        public TAggregateRootkey AggregateRootId => aggregateRootId;
        public TState State => (TState)(aggregateRoot.UntypedState as IAggregateRootState).ChildStates.First(x => x.Id?.Equals(Id) == true && x.GetType() == typeof(TState));

        protected TAggregateRootkey aggregateRootId { get => (TAggregateRootkey)aggregateRoot.Id; }

        private IAggregateRoot aggregateRoot;

        protected ChildEntity()
        {
        }

        protected ChildEntity(IAggregateRoot aggregateRoot, IEnumerable<IChildEntityEvent> events)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoot));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            this.aggregateRoot = aggregateRoot;

            Id = (TChildKey)new TState().ConvertFromStreamName(typeof(TChildKey), events.First().Id.ToString());

            foreach (var @event in events)
            {
                (this.aggregateRoot as IEventSourcedEntityInternal).RaiseEvent(@event);
            }
        }

        protected ChildEntity(IAggregateRoot aggregateRoot, IChildEntityEvent @event)
            : this(aggregateRoot, new[] { @event })
        {

        }

        public override bool Equals(object obj)
        {
            TState otherState = null;

            if (obj is ChildEntity<TState, TAggregateRootkey, TChildKey> other)
            {
                otherState = other.State;
            }
            else if (obj is TState state)
            {
                otherState = state;
            }

            if (ReferenceEquals(otherState, null))
            {
                return false;
            }

            return State.Equals(otherState);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }

        protected void RaiseEvent(IEvent @event)
        {
            (aggregateRoot as IEventSourcedEntityInternal).RaiseEvent(@event);
        }

        void IChildEntityInternal.SetAggregateRoot(IAggregateRoot aggregateRoot, object id)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoot));
            }

            if (this.aggregateRoot != null)
            {
                throw new InvalidOperationException("AggregateRoot cannot be reassigned in ChildEntity!");
            }

            if (id is string)
            {
                id = new TState().ConvertFromStreamName(typeof(TChildKey), (string)id);
            }

            this.aggregateRoot = aggregateRoot;
            Id = (TChildKey)id;
        }
    }
}
