﻿using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public class EventRepository : IEventRepository
    {
        protected readonly ISubject<IMessage> committedMessagesSubject = new Subject<IMessage>();
        protected readonly IInstanceProvider instanceProvider;
        protected readonly IPersistenceEngine persistenceEngine;

        private readonly IRawStreamEntryFactory rawStreamEntryFactory;

        public EventRepository(IInstanceProvider instanceProvider, IPersistenceEngine persistenceEngine, IRawStreamEntryFactory rawStreamEntryFactory)
        {
            this.instanceProvider = instanceProvider;
            this.persistenceEngine = persistenceEngine;
            this.rawStreamEntryFactory = rawStreamEntryFactory;
        }

        public virtual async Task<TEventSourcedEntity> GetAsync<TEventSourcedEntity>(string streamName, int minRevision = 0, int maxRevision = int.MaxValue)
                where TEventSourcedEntity : class, IEventSourcedEntity
        {
            return (TEventSourcedEntity)await GetAsync(typeof(TEventSourcedEntity), streamName, minRevision, maxRevision);
        }

        public virtual async Task<IEventSourcedEntity> GetAsync(Type aggregateType, string streamName, int minRevision = 0, int maxRevision = int.MaxValue)
        {
            if (streamName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(streamName));
            }

            var streamEntries = await persistenceEngine
                .LoadStreamEntriesByStreamAsync(streamName)
                .ToListAsync();

            if (streamEntries.Count == 0)
            {
                return null;
            }

            var instance = (IEventSourcedEntity)instanceProvider.GetInstance(aggregateType);

            var events = new List<IEvent>();
            for (var i = 0; i < streamEntries.Count; i++)
            {
                var rawStreamEntry = streamEntries[i];

                events.Add((IEvent)persistenceEngine.Serializer.Deserialize(rawStreamEntry.Payload));
            }

            instance.LoadEvents(events);

            return instance;
        }

        public virtual async Task<int> SaveAsync(IEnumerable<IEventSourcedEntity> entities, IDictionary<string, object> commitHeaders = null)
        {
            var commitId = Guid.NewGuid().ToString();
            int result;
            var allMessages = new List<IMessage>();
            var allStreamEntries = new List<IRawStreamEntry>();

            commitHeaders = commitHeaders ?? new Dictionary<string, object>();

            var distinctEntitiesList = entities
                .GroupBy(x => new { x.Id, x.GetType().Name })
                .Select(x => x.First())
                .ToList();

            foreach (var entity in distinctEntitiesList)
            {
                var streamName = entity.StreamName;

                var baseStreamRevision = entity.Version - entity.UncommittedEvents.Count();

                var messages = entity.UncommittedEvents
                    .Select(@event => @event.ToTypedMessage(
                                   Guid.NewGuid().ToString(),
                                   commitHeaders,
                                   commitHeaders?.ContainsKey(MessageConstants.CorrelationIdKey) == true ?
                                        (commitHeaders[MessageConstants.CorrelationIdKey] ?? "").ToString() :
                                        null,
                                   commitHeaders?.ContainsKey(MessageConstants.CausationIdKey) == true ?
                                        (commitHeaders[MessageConstants.CausationIdKey] ?? "").ToString() :
                                        null,
                                   DateTime.UtcNow,
                                   0))
                    .ToList();

                allMessages.AddRange(messages);

                var streamDTOs = messages
                    .Select(x => rawStreamEntryFactory.CreateRawStreamEntry(persistenceEngine.Serializer, streamName, commitId, ++baseStreamRevision, x))
                    .ToList();

                allStreamEntries.AddRange(streamDTOs);
            }

            result = await persistenceEngine.SaveStreamEntriesAsync(allStreamEntries);
            allStreamEntries.Clear();

            foreach (var entity in entities)
            {
                entity.ClearUncommittedEvents();
            }

            foreach (var message in allMessages)
            {
                committedMessagesSubject.OnNext(message);
            }

            allMessages.Clear();

            return result;
        }

        public virtual async Task<int> SaveAsync(IEventSourcedEntity entity, IDictionary<string, object> commitHeaders = null)
        {
            return await SaveAsync(new[] { entity }, commitHeaders);
        }

        public virtual Task<int> GetCurrentCheckpointNumberAsync()
        {
            return persistenceEngine.GetCurrentEventStoreCheckpointNumberAsync();
        }

        public virtual IObservable<T> SubscribeTo<T>()
            where T : class, IMessage
        {
            return 
                committedMessagesSubject
                .Where(m =>
                    (
                        m is T || m.Body is T
                    ))
                    .Select(m =>
                        typeof(IMessage).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()) ? (T)m : (T)m.Body
                        );
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                (committedMessagesSubject as Subject<IMessage>)?.Dispose();
            }
        }
    }
}

