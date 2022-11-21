using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Tests.Domain.TestData
{
    public class ParentEntity : AggregateRoot<ParentState, ParentEntityId>
    {
        public ParentEntity(ParentEntityId id, string name)
            : base(new ParentCreated(id, name, DateTime.UtcNow))
        {

        }
        public ParentEntity(IEvent @event)
            : base(new[] { @event })
        {

        }
        public ParentEntity(IEnumerable<IEvent> events)
            : base(events)
        {

        }

        public ParentEntity(IEnumerable<IEvent> events, ParentState initialState)
            : base(events, initialState)
        {

        }
    }
}
