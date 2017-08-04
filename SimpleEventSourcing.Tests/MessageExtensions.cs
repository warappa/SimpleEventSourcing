using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Tests
{
	public static class MessageExtensions
	{
		public static IMessage<TEvent> ToEventMessage<TEvent>(this TEvent @event)
			where TEvent : class, IEvent
		{
			return new EventMessage<TEvent>(Guid.NewGuid().ToString(), @event, new Dictionary<string, object>(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), DateTime.UtcNow, 0);
		}

		public static IMessage<TCommand> ToCommandMessage<TCommand>(this TCommand command)
			where TCommand : class, ICommand
		{
			return new CommandMessage<TCommand>(Guid.NewGuid().ToString(), command, new Dictionary<string, object>(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), DateTime.UtcNow, 0);
		}
	}
}
