using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Tests
{
    public class ChildEntity : ChildEntity<ChildState, ParentEntityId, ChildEntityId>
    {
        public ChildEntity() : base() { }
        public ChildEntity(IAggregateRoot aggregateRoot, ChildEntityId id, string name)
            : base(aggregateRoot, new ChildCreated((ParentEntityId)aggregateRoot.Id, id, name, DateTime.UtcNow))
        {

        }
        public ChildEntity(IAggregateRoot aggregateRoot, IEnumerable<IChildEntityEvent> events)
            : base(aggregateRoot, events)
        {
        }

        public void Rename(string newName)
        {
            RaiseEvent(new ChildRenamed(AggregateRootId, Id, newName, DateTime.UtcNow));
        }
    }
}
