using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Tests
{
	public interface IEventWithId : IEvent
	{
		string Id { get; }
	}
}