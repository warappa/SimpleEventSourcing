using System;

namespace SimpleEventSourcing.Bus
{
    public interface IObservableMessageBus : IEventMessagePublisher, ICommandMessageSender, IObservableMessageSource, IDisposable
    {

    }
}
