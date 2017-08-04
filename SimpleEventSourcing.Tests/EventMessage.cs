using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Tests
{

	public class EventMessage<TEvent> : Message, IMessage<TEvent>
		where TEvent : class, IEvent
	{
		public EventMessage(string messageId, object body, IDictionary<string, object> headers, string correlationId, string causationId, DateTime dateTime, int checkpointNumber)
			: base(messageId, body, headers, correlationId, causationId, dateTime, checkpointNumber)
		{

		}

		TEvent IMessage<TEvent>.Body
		{
			get { return (TEvent)Body; }
		}
	}
}