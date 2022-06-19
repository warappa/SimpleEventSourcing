using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleEventSourcing.Messaging
{
    public static class MessageExtensions
    {
        private static TypeInfo typedMessageTypeInfo = typeof(TypedMessage<>).GetTypeInfo();

        public static IMessage ToTypedMessage(this object body, string messageId, IDictionary<string, object> headers, string correlationId, string causationId, DateTime dateTime, int checkpointNumber)
        {
            return (IMessage)Activator.CreateInstance(typedMessageTypeInfo.MakeGenericType(body.GetType()),
                            messageId,
                            body,
                            headers,
                            correlationId,
                            causationId,
                            dateTime,
                            checkpointNumber);
        }
    }
}
