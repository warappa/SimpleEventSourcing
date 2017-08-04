using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.WriteModel
{
    public interface IEventRepository : IDisposable
    {
        TEventSourcedEntity Get<TEventSourcedEntity>(string streamName, int minRevision = 0, int maxRevision = int.MaxValue)
                where TEventSourcedEntity : class, IEventSourcedEntity;
        IEventSourcedEntity Get(Type aggregateType, string streamName, int minRevision = 0, int maxRevision = int.MaxValue);

        int Save(IEventSourcedEntity entity, IDictionary<string, object> commitHeaders = null);
        IObservable<T> SubscribeTo<T>()
            where T : class, IMessage;

        int GetCurrentCheckpointNumber();
    }
}
