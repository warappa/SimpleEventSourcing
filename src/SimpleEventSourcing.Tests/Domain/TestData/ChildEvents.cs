using System;

namespace SimpleEventSourcing.Tests
{
    public class ChildCreated : ChildBaseEvent
    {
        public ChildCreated(string aggregateRootId, string id, string name, DateTime dateTime)
            : base(aggregateRootId, id, dateTime)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class ChildRenamed : ChildBaseEvent
    {
        public ChildRenamed(string aggregateRootId, string id, string name, DateTime dateTime)
            : base(aggregateRootId, id, dateTime)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}