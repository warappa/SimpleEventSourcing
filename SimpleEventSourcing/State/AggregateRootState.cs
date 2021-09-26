using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleEventSourcing.State
{
    public abstract class AggregateRootState<TState, TKey> : StreamState<TState>, IAggregateRootState<TKey>, IAggregateRootStateInternal
        where TState : AggregateRootState<TState, TKey>, new()
    {
        protected IDictionary<Type, Func<object, IChildEventSourcedState>> childStateCreationMap = new Dictionary<Type, Func<object, IChildEventSourcedState>>();
        private List<IChildEventSourcedState> childStates = new();
        [JsonInclude]
        public IEnumerable<IChildEventSourcedState> ChildStates { get => childStates; private set => childStates = value.ToList(); }
        IDictionary<Type, Func<object, IChildEventSourcedState>> IAggregateRootState.ChildStateCreationMap => childStateCreationMap;
        public TKey Id
        {
            get
            {
                if (EqualityComparer<TKey>.Default.Equals(id, default))
                {
                    id = (TKey)ConvertFromStreamName(typeof(TKey), StreamName);
                }

                return id;
            }
            set
            {
                if (EqualityComparer<TKey>.Default.Equals(value, id))
                {
                    return;
                }

                if (!EqualityComparer<TKey>.Default.Equals(id, default))
                {
                    throw new InvalidOperationException("Id may not be changed!");
                }

                id = value;
                StreamName = ConvertToStreamName(typeof(TKey), value);
            }
        }

        private TKey id;

        object IAggregateRootState.Id => Id;

        protected AggregateRootState() { }

        protected AggregateRootState(TState state)
        {
            StreamName = state.StreamName;
            childStateCreationMap = ((IAggregateRootState)state).ChildStateCreationMap;
            childStates = state.ChildStates.ToList();
        }

        public TChildState GetChildState<TChildState>(object id)
            where TChildState : IChildEventSourcedState
        {
            return ChildStates.OfType<TChildState>().FirstOrDefault(x => x.Id?.Equals(id) == true);
        }

        protected override TState InvokeAssociatedApply(object eventOrMessage)
        {
            var state = base.InvokeAssociatedApply(eventOrMessage);

            if (eventOrMessage is IMessage)
            {
                if ((eventOrMessage as IMessage).Body is IChildEntityEvent)
                {
                    ApplyChildEvent((eventOrMessage as IMessage).Body as IChildEntityEvent);
                }
            }
            else if (eventOrMessage is IChildEntityEvent)
            {
                ApplyChildEvent(eventOrMessage as IChildEntityEvent);
            }

            return state;
        }

        protected void ApplyChildEvent(IChildEntityEvent @event)
        {
            IChildEventSourcedState childState = null;

            if (childStateCreationMap.TryGetValue(@event.GetType(), out var creator))
            {
                childState = creator(@event).ExtractState<IChildEventSourcedState>();
                childStates.Add(childState);
            }
            else
            {
                childState = ChildStates.FirstOrDefault(x => x.Id.Equals(@event.Id));

                var newState = childState.UntypedApply(@event).ExtractState<IChildEventSourcedState>();

                if (newState != childState)
                {
                    childStates.Remove(childState);
                    childStates.Add(newState);
                }
            }
        }

        public void AddChildState(IChildEventSourcedState childState)
        {
            childStates.Add(childState);
        }

        public void RemoveChildState(IChildEventSourcedState childState)
        {
            childStates.Remove(childState);
        }
    }
}