using SimpleEventSourcing.Messaging;
using System;
using System.Reactive.Subjects;

namespace SimpleEventSourcing.Bus
{
    public class ObservableMessageBus : IObservableMessageBus
    {
        public IObservable<IMessage> ObservableSource => messageSubject;

        internal readonly Subject<IMessage> messageSubject = new Subject<IMessage>();

        public void Publish<T>(T message)
            where T : IMessage<IEvent>
        {
            messageSubject.OnNext(message);
        }

        public void Send<T>(T message)
            where T : IMessage<ICommand>
        {
            messageSubject.OnNext(message);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                messageSubject?.Dispose();
            }
        }
    }
}
