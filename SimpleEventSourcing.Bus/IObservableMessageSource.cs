using SimpleEventSourcing.Messaging;
using System;

namespace SimpleEventSourcing.Bus
{
    public interface IObservableMessageSource
    {
        IObservable<IMessage> ObservableSource { get; }
    }
}
