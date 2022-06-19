using SimpleEventSourcing.Messaging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

namespace SimpleEventSourcing.Bus
{
    public static class BusExtensions
    {
        public static IObservable<T> SubscribeTo<T>(this IObservableMessageBus bus)
        {
            return ((ObservableMessageBus)bus)
                .messageSubject
                .Where(m => m is T || m.Body is T)
                    .Select(m => typeof(IMessage).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()) ?
                        (T)m :
                        (T)m.Body
                        );
        }

        public static IObservable<T> All<T>(this IObservableMessageBus bus)
            where T : IMessage
        {
            return ((ObservableMessageBus)bus)
                .messageSubject
                .Where(m => m is T)
                .Select(m => (T)m);
        }
    }
}
