using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.State
{
    public interface IAggregateRootState
    {
        IEnumerable<IChildEventSourcedState> ChildStates { get; }
        IDictionary<Type, Func<object, IChildEventSourcedState>> ChildStateCreationMap { get; }
        TChildState GetChildState<TChildState>(object id)
            where TChildState : IChildEventSourcedState;

        object Id { get; }
    }

    public interface IAggregateRootStateInternal : IAggregateRootState
    {
        void AddChildState(IChildEventSourcedState childState);
        void RemoveChildState(IChildEventSourcedState childState);
    }
}
