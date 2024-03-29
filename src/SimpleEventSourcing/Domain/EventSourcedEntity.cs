﻿using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.Domain
{
    public class EventSourcedEntity<TState, TKey> : IEventSourcedEntity<TState, TKey>, IEventSourcedEntityInternal
        where TState : class, IStreamState<TState>, ISynchronousEventSourcedState<TState>, new()
    {
        public TKey Id => (TKey)State.ConvertFromStreamName(typeof(TKey), state.StreamName);
        public TState State => SynchronousEventSourcedState<TState>.LoadState(state);

        protected int committedVersion;
        protected int version;
        protected TState state;
        protected IList<IEvent> uncommittedEvents = new List<IEvent>();

        IEnumerable<IEvent> IEventSourcedEntity.UncommittedEvents => uncommittedEvents;
        int IEventSourcedEntity.Version => version;
        object IEventSourcedEntity.Id => Id;
        object IEventSourcedEntity.UntypedState => state;
        string IEventSourcedEntity.StreamName => State.StreamName;

        public EventSourcedEntity(IEnumerable<IEvent> events, TState initialState = null, int version = 0)
        {
            (this as IEventSourcedEntity<TState, TKey>).LoadEvents(events, initialState, version);

            this.version = version + events.Count();
            committedVersion = this.version;
        }

        public EventSourcedEntity(IEvent @event, TState initialState = null, int version = 0)
        {
            state = SynchronousEventSourcedState<TState>.LoadState(initialState, new[] { @event });
            uncommittedEvents.Add(@event);

            this.version = version + 1;
            committedVersion = this.version;
        }

        public override bool Equals(object obj)
        {
            TState otherState = null;

            if (obj is EventSourcedEntity<TState, TKey> other)
            {
                otherState = other.State;
            }
            else if (obj is TState state)
            {
                otherState = state;
            }

            if (otherState is null)
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
            state = SynchronousEventSourcedState<TState>.LoadState(state, new[] { @event });

            if (@event is IChildEntityEvent &&
                this is IAggregateRoot)
            {
                var aggregateRootState = state as IAggregateRootStateInternal;
                var childEvent = @event as IChildEntityEvent;

                var originalChildState = aggregateRootState.ChildStates.FirstOrDefault(x => x.Id.Equals(childEvent.Id));

                var childState = originalChildState ?? aggregateRootState.ChildStateCreationMap[@event.GetType()](@event);

                var newState = childState.UntypedApply(@event);

                if (newState != originalChildState)
                {
                    if (originalChildState != null)
                    {
                        aggregateRootState.RemoveChildState(originalChildState);
                    }

                    aggregateRootState.AddChildState((IChildEventSourcedState)newState);
                }
            }

            uncommittedEvents.Add(@event);
            version++;
        }

        void IEventSourcedEntity.LoadEvents(IEnumerable<IEvent> events, object initialState, int version)
        {
            (this as IEventSourcedEntity<TState, TKey>).LoadEvents(events, (TState)initialState, version);
        }

        void IEventSourcedEntity<TState, TKey>.LoadEvents(IEnumerable<IEvent> events, TState initialState, int version)
        {
            state = SynchronousEventSourcedState<TState>.LoadState(initialState, events);

            committedVersion = this.version = version + events.Count();
        }

        void IEventSourcedEntity.ClearUncommittedEvents()
        {
            committedVersion = version;
            uncommittedEvents.Clear();
        }

        void IEventSourcedEntityInternal.RaiseEvent(IEvent @event)
        {
            RaiseEvent(@event);
        }
    }
}
