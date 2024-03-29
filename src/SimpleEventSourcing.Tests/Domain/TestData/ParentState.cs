﻿using SimpleEventSourcing.State;
using System;

namespace SimpleEventSourcing.Tests.Domain.TestData
{

    public class ParentState : AggregateRootState<ParentState, ParentEntityId>, IStreamState<ParentState>
    {
        public string Name { get; private set; }

        static ParentState()
        {
            childStateCreationMap.Add(typeof(ChildCreated), evt => new ChildState().Apply(evt as ChildCreated));
        }

        public ParentState Apply(ParentCreated @event)
        {
            StreamName = @event.Id;
            Name = @event.Name;

            return this;
        }

        public override object ConvertFromStreamName(Type tkey, string streamName)
        {
            return new ParentEntityId(streamName);
        }

        public override string ConvertToStreamName(Type tkey, object id)
        {
            return ((ParentEntityId)id).Value;
        }
    }
}
