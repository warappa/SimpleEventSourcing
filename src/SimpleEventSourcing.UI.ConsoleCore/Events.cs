﻿using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    [Versioned("TestAggregateCreated", 0)]
    public class TestAggregateCreated : IEvent, INameChangeEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public TestAggregateCreated(
            string id,
            string name)
        {
            Id = id;
            Name = name;
        }

        string INameChangeEvent.Name => Name;
    }

    [Versioned("Renamed", 0)]
    public class Renamed : IEvent, INameChangeEvent
    {
        public string Id { get; set; }
        public string NewName { get; set; }

        public Renamed(
            string id,
            string newName)
        {
            Id = id;
            NewName = newName;
        }

        public string Name => NewName;
    }

    [Versioned("SomethingDone", 0)]
    public class SomethingDone : IEvent
    {
        public string Id { get; set; }
        public string Bla { get; set; }

        public SomethingDone(
            string id,
            string bla)
        {
            Id = id;
            Bla = bla;
        }
    }
    [Versioned("SomethingSpecialDone", 0)]
    public class SomethingSpecialDone : IEvent
    {
        public string Id { get; set; }
        public string Bla { get; set; }

        public SomethingSpecialDone(
            string id,
            string bla)
        {
            Id = id;
            Bla = bla;
        }
    }

    [Versioned("EventNotHandledByPersistentState", 0)]
    public class EventNotHandledByPersistentState : IEvent
    {
        public string Id { get; set; }
        public bool NotHandled { get; set; }

        public EventNotHandledByPersistentState(
            string id,
            bool notHandled)
        {
            Id = id;
            NotHandled = notHandled;
        }
    }
}
