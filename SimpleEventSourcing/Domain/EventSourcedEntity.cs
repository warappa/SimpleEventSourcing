using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.Domain
{
    public class EventSourcedEntity<TState, TKey> : IEventSourcedEntity<TState, TKey>, IEventSourcedEntityInternal
        where TState : class, IStreamState<TState>, IEventSourcedState<TState>, new()
    {
        public TKey Id => (TKey)StateModel.ConvertFromStreamName(typeof(TKey), stateModel.StreamName);
        public TState StateModel => EventSourcedState<TState>.LoadState(this.stateModel);

        protected int committedVersion;
        protected int version;
        protected TState stateModel;
        protected IList<IEvent> uncommittedEvents = new List<IEvent>();

        IEnumerable<IEvent> IEventSourcedEntity.UncommittedEvents => uncommittedEvents;
        int IEventSourcedEntity.Version => version;
        object IEventSourcedEntity.Id => Id;
        object IEventSourcedEntity.UntypedStateModel => stateModel;
        string IEventSourcedEntity.StreamName => StateModel.StreamName;

        public EventSourcedEntity(IEnumerable<IEvent> events, TState initialState = null)
        {
            (this as IEventSourcedEntity<TState, TKey>).LoadEvents(events, initialState);
        }

        public EventSourcedEntity(IEvent @event, TState initialState = null)
        {
            stateModel = EventSourcedState<TState>.LoadState(initialState, new[] { @event });
            uncommittedEvents.Add(@event);
            version = 1;
        }

        public override bool Equals(object obj)
        {
            TState otherState = null;

            if (obj is EventSourcedEntity<TState, TKey> other)
            {
                otherState = other.StateModel;
            }
            else if (obj is TState state)
            {
                otherState = state;
            }

            if (ReferenceEquals(otherState, null))
            {
                return false;
            }

            return StateModel.Equals(otherState);
        }

        public override int GetHashCode()
        {
            return StateModel.GetHashCode();
        }

        protected void RaiseEvent(IEvent @event)
        {
            stateModel = EventSourcedState<TState>.LoadState(stateModel, new[] { @event });

            if (@event is IChildEntityEvent &&
                this is IAggregateRoot)
            {
                var aggregateRootState = stateModel as IAggregateRootStateInternal;
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

        void IEventSourcedEntity.LoadEvents(IEnumerable<IEvent> events, object initialState)
        {
            (this as IEventSourcedEntity<TState, TKey>).LoadEvents(events, (TState)initialState);
        }

        void IEventSourcedEntity<TState, TKey>.LoadEvents(IEnumerable<IEvent> events, TState initialState)
        {
            committedVersion = version = events.Count();

            stateModel = EventSourcedState<TState>.LoadState(initialState, events);
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
