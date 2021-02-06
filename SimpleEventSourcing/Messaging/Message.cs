using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Messaging
{
    public class Message : IMessage
    {
        public IDictionary<string, object> Headers { get; protected set; }

        public object Body { get; protected set; }
        public DateTime DateTime { get { return (DateTime)Headers[MessageConstants.DateTimeKey]; } protected set { Headers[MessageConstants.DateTimeKey] = value; } }
        public string MessageId { get { return (string)Headers[MessageConstants.MessageIdKey]; } protected set { Headers[MessageConstants.MessageIdKey] = value; } }
        public string CorrelationId { get { return (string)Headers[MessageConstants.CorrelationIdKey]; } protected set { Headers[MessageConstants.CorrelationIdKey] = value; } }
        public string CausationId { get { return (string)Headers[MessageConstants.CausationIdKey]; } protected set { Headers[MessageConstants.CausationIdKey] = value; } }
        public int CheckpointNumber { get { return (int)Headers[MessageConstants.CheckpointNumberKey]; } protected set { Headers[MessageConstants.CheckpointNumberKey] = value; } }

        public Message(string messageId, object body, IDictionary<string, object> headers, string correlationId, string causationId, DateTime dateTime, int checkpointNumber)
        {
            Headers = headers is object ? new Dictionary<string, object>(headers) : new Dictionary<string, object>();

            MessageId = messageId;
            Body = body;
            DateTime = dateTime;
            CorrelationId = correlationId;
            CausationId = causationId;
            CheckpointNumber = checkpointNumber;
        }
    }
}
