using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleEventSourcing.WriteModel
{
    public static class IRawStreamEntryExtensions
    {
        private static TypeInfo typedMessageTypeInfo = typeof(TypedMessage<>).GetTypeInfo();

        public static IMessage ToMessage(this IRawStreamEntry streamDTO, ISerializer serializer)
        {
            var headers = serializer.Deserialize<IDictionary<string, object>>(streamDTO.Headers);

            return new Message(
                streamDTO.MessageId,
                serializer.Deserialize<IEvent>(streamDTO.Payload),
                headers,
                headers.ContainsKey(MessageConstants.CorrelationIdKey) ?
                                (string)headers[MessageConstants.CorrelationIdKey] : null,
                headers.ContainsKey(MessageConstants.CausationIdKey) ?
                                (string)headers[MessageConstants.CausationIdKey] : null,
                DateTime.UtcNow,
                streamDTO.CheckpointNumber);
        }

        public static IMessage ToTypedMessage(this IRawStreamEntry rawStreamEntry, ISerializer serializer)
        {
            var headers = serializer.Deserialize<IDictionary<string, object>>(rawStreamEntry.Headers);
            var body = serializer.Deserialize<IEvent>(rawStreamEntry.Payload);

            var messageType = typedMessageTypeInfo.MakeGenericType(body.GetType());

            return (IMessage)Activator.CreateInstance(messageType,
                            rawStreamEntry.MessageId,
                            body,
                            headers,
                            headers?.ContainsKey(MessageConstants.CorrelationIdKey) == true ?
                                            headers[MessageConstants.CorrelationIdKey] : null,
                            headers?.ContainsKey(MessageConstants.CausationIdKey) == true ?
                                            headers[MessageConstants.CausationIdKey] : null,
                            DateTime.UtcNow,
                            rawStreamEntry.CheckpointNumber);
        }
    }
}
