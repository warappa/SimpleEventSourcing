using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Tests
{
    public class DummyMessage<T> : TypedMessage<T>
        where T : class
    {
        public DummyMessage(string correlationId, T body)
            : base(Guid.NewGuid().ToString(), body, new Dictionary<string, object>(), correlationId, Guid.NewGuid().ToString(), DateTime.UtcNow, 0)
        {

        }
    }
}