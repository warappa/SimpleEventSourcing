using System;

namespace SimpleEventSourcing.Tests
{
    public class ParentCreated : BaseEvent
    {
        public ParentCreated(string id, string name, DateTime dateTime)
            : base(id, dateTime)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class ParentRenamed : BaseEvent
    {
        public ParentRenamed(string id, string name, DateTime dateTime)
            : base(id, dateTime)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}