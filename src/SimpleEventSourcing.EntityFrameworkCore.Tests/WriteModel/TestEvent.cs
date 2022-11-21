using SimpleEventSourcing.Messaging;
using System;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel
{
    public class TestEvent : IEvent
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }
}
