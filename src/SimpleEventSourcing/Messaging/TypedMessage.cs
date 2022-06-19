using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Messaging
{
    public class TypedMessage<TEvent> : Message, IMessage<TEvent>
        where TEvent : class
    {
        public TypedMessage(string messageId, object body, IDictionary<string, object> headers, string correlationId, string causationId, DateTime dateTime, int checkpointNumber)
            : base(messageId, body, headers, correlationId, causationId, dateTime, checkpointNumber)
        {

        }

        TEvent IMessage<TEvent>.Body
        {
            get { return (TEvent)Body; }
        }
    }
}
