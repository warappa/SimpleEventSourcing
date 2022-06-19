using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Messaging
{
    public interface IMessage
    {
        object Body { get; }
        IDictionary<string, object> Headers { get; }
        DateTime DateTime { get; }
        string CorrelationId { get; }
        string CausationId { get; }
        string MessageId { get; }
        int CheckpointNumber { get; }
    }
}
