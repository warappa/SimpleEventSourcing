using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Tests.Domain.TestData
{
    public interface IEventWithId : IEvent
    {
        string Id { get; }
    }
}
