using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static IEnumerable<IMessage> ToMessages(this IEnumerable<IRawStreamEntry> rawStremEntries, ISerializer serializer)
        {
            return rawStremEntries.Select(x => x.ToMessage(serializer));
        }

        public static IMessage ToTypedMessage(this IRawStreamEntry rawStreamEntry, ISerializer serializer)
        {
            var headers = serializer.Deserialize<IDictionary<string, object>>(rawStreamEntry.Headers);
            var payloadType = serializer.Binder.BindToType(rawStreamEntry.PayloadType);
            var body = (IEvent)serializer.Deserialize(payloadType, rawStreamEntry.Payload);

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

        public static IEnumerable<IMessage> ToTypedMessages(this IEnumerable<IRawStreamEntry> rawStremEntries, ISerializer serializer)
        {
            return rawStremEntries.Select(x => x.ToTypedMessage(serializer));
        }

        public static IEvent ToEvent(this IRawStreamEntry rawStreamEntry, ISerializer serializer)
        {
            var payloadType = serializer.Binder.BindToType(rawStreamEntry.PayloadType);
            return (IEvent)serializer.Deserialize(payloadType, rawStreamEntry.Payload);
        }

        public static IEnumerable<IEvent> ToEvents(this IEnumerable<IRawStreamEntry> rawStremEntries, ISerializer serializer)
        {
            return rawStremEntries.Select(x => x.ToEvent(serializer));
        }

        public static TState ApplyToState<TState>(this IEnumerable<IRawStreamEntry> entries, TState state, ISerializer serializer)
            where TState : class, ISynchronousEventSourcedState<TState>
        {
            foreach (var e in entries)
            {
                //snapShot10.Apply((TestEvent)e.ToMessage(persistenceEngine.Serializer).Body);
                state = state.Apply(e.ToEvent(serializer)) ?? state;
            }

            return state;
        }

        public static TState ApplyToState<TState>(this IEnumerable<IEvent> events, TState state)
            where TState : class, ISynchronousEventSourcedState<TState>
        {
            foreach (var e in events)
            {
                //snapShot10.Apply((TestEvent)e.ToMessage(persistenceEngine.Serializer).Body);
                state = state.Apply(e) ?? state;
            }

            return state;
        }
    }
}
