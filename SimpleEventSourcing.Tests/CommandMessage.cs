using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Tests
{

    public class CommandMessage<TCommand> : Message, IMessage<TCommand>//, ICommandMessage<TCommand>
		where TCommand : class, ICommand
	{
		public CommandMessage(string messageId, object body, IDictionary<string, object> headers, string correlationId, string causationId, DateTime dateTime, int checkpointNumber)
			: base(messageId, body, headers, correlationId, causationId, dateTime, checkpointNumber)
		{

		}

		TCommand IMessage<TCommand>.Body
		{
			get { return (TCommand)base.Body; }
		}
	}
}