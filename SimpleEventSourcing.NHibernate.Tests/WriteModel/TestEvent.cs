﻿using SimpleEventSourcing.Messaging;
using System;

namespace SimpleEventSourcing.NHibernate.Tests
{
    public class TestEvent : IEvent
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }
}