using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IEventRepository : IDisposable
    {
        Task<TEventSourcedEntity> GetAsync<TEventSourcedEntity>(string streamName, int minRevision = 0, int maxRevision = int.MaxValue)
                where TEventSourcedEntity : class, IEventSourcedEntity;
        Task<IEventSourcedEntity> GetAsync(Type aggregateType, string streamName, int minRevision = 0, int maxRevision = int.MaxValue);

        Task<int> SaveAsync(IEnumerable<IEventSourcedEntity> entities, IDictionary<string, object> commitHeaders = null);
        Task<int> SaveAsync(IEventSourcedEntity entity, IDictionary<string, object> commitHeaders = null);
        IObservable<T> SubscribeTo<T>()
            where T : class, IMessage;

        Task<int> GetCurrentCheckpointNumberAsync();
    }
}
