using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using SimpleEventSourcing.NHibernate.ReadModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.WriteModel
{
    public class PersistenceEngine : IPersistenceEngine
    {
        public const string PayloadTypesSeparator = "~";
        private readonly ISessionFactory sessionFactory;
        private readonly Configuration configuration;
        private readonly int batchSize;

        public ISerializer Serializer { get; }

        public PersistenceEngine(ISessionFactory sessionFactory, Configuration configuration, ISerializer serializer)
            : this(sessionFactory, configuration, serializer, 1000)
        {
        }

        public PersistenceEngine(ISessionFactory sessionFactory, Configuration configuration, ISerializer serializer, int batchSize)
        {
            this.sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.batchSize = batchSize;
        }

        public Task InitializeAsync()
        {
            var schemaExport = new SchemaUpdate(configuration);
            try
            {
                schemaExport.Execute(true, true);
            }
            catch
            {
                // TODO: error handling
            }

            return Task.CompletedTask;
        }

        public IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesByStreamAsync(GroupConstants.All, null, streamName, minRevision, maxRevision, payloadTypes, ascending, take);
        }

        public async IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string group, string category, string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            List<RawStreamEntry> rawStreamEntries = null;
            
            var payloadValues = GetPayloadValues(payloadTypes);

            while (true)
            {
                using (var scope = OpenScope())
                using (var statelessSession = sessionFactory.OpenStatelessSession())
                using (var transaction = statelessSession.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        var query = statelessSession.Query<RawStreamEntry>();

                        query = query.Where(x => x.StreamRevision >= minRevision && x.StreamRevision <= maxRevision);

                        if (!string.IsNullOrWhiteSpace(streamName))
                        {
                            query = query.Where(x => x.StreamName == streamName);
                        }

                        if (payloadValues is object)
                        {
                            query = query.Where(x => payloadValues.Contains(x.PayloadType));
                        }

                        if (group != null &&
                            group != GroupConstants.All)
                        {
                            query = query.Where(x => x.Group == group);
                        }

                        if (category != null)
                        {
                            query = query.Where(x => x.Category == category);
                        }

                        if (ascending)
                        {
                            query = query.OrderBy(x => x.CheckpointNumber);
                        }
                        else
                        {
                            query = query.OrderByDescending(x => x.CheckpointNumber);
                        }

                        var nextBatchSize = Math.Min(take - taken, batchSize);

                        query = query.Take(nextBatchSize);

                        rawStreamEntries = await query.ToListAsync();
                    }
                    finally
                    {
                        transaction.Commit();
                    }
                }

                if (rawStreamEntries.Count == 0)
                {
                    yield break;
                }

                foreach (var streamEntry in rawStreamEntries)
                {
                    yield return streamEntry;
                }

                taken += rawStreamEntries.Count;

                if (taken >= take)
                {
                    yield break;
                }

                if (ascending)
                {
                    minRevision = rawStreamEntries[rawStreamEntries.Count - 1].StreamRevision + 1;
                }
                else
                {
                    maxRevision -= take;
                }
            }
        }

        public IDisposable OpenScope()
        {
            if (CurrentSessionContext.HasBind(sessionFactory))
            {
                return new NullDisposable();
            }

            var session = sessionFactory.OpenSession();
            session.BeginTransaction();

            CurrentSessionContext.Bind(session);

            return new CurrentSessionContextDisposer(sessionFactory);
        }

        public IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesAsync(GroupConstants.All, null, minCheckpointNumber, maxCheckpointNumber, payloadTypes, ascending, take);
        }

        public async IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(string group, string category, int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            List<RawStreamEntry> rawStreamEntries = null;
            
            var payloadValues = GetPayloadValues(payloadTypes);

            if (!ascending &&
                maxCheckpointNumber == int.MaxValue)
            {
                maxCheckpointNumber = await GetCurrentEventStoreCheckpointNumberAsync();
            }

            while (true)
            {
                //using (var scope = OpenScope())
                using (var statelessSession = sessionFactory.OpenStatelessSession())
                using (var transaction = statelessSession.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    var query = statelessSession.Query<RawStreamEntry>();

                    query = query.Where(x => x.CheckpointNumber >= minCheckpointNumber && x.CheckpointNumber <= maxCheckpointNumber);

                    if (payloadValues is object)
                    {
                        query = query.Where(x => payloadValues.Contains(x.PayloadType));
                    }

                    if (group != null &&
                         group != GroupConstants.All)
                    {
                        query = query.Where(x => x.Group == group);
                    }

                    if (category != null)
                    {
                        query = query.Where(x => x.Category == category);
                    }

                    if (ascending)
                    {
                        query = query.OrderBy(x => x.CheckpointNumber);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.CheckpointNumber);
                    }

                    var nextBatchSize = Math.Min(take - taken, batchSize);

                    query = query.Take(nextBatchSize);

                    try
                    {
                        // TODO: utilize async enumerable
                        rawStreamEntries = await query.ToListAsync();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());

                        throw;
                    }

                    transaction.Commit();
                }

                if (rawStreamEntries.Count == 0)
                {
                    yield break;
                }

                foreach (var streamEntry in rawStreamEntries)
                {
                    yield return streamEntry;
                }

                taken += rawStreamEntries.Count;

                if (taken >= take)
                {
                    yield break;
                }

                if (ascending)
                {
                    minCheckpointNumber = rawStreamEntries[rawStreamEntries.Count - 1].CheckpointNumber + 1;
                }
                else
                {
                    maxCheckpointNumber -= take;
                }
            }
        }

        public async Task<int> SaveStreamEntriesAsync(IEnumerable<IRawStreamEntry> entries)
        {
            int result;

            //using (var scope = OpenScope())
            using (var statelessSession = sessionFactory.OpenStatelessSession())
            using (var transaction = statelessSession.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                statelessSession.SetBatchSize(batchSize);

                var commitId = Guid.NewGuid().ToString();

                foreach (var rawStreamEntry in entries)
                {
                    rawStreamEntry.CommitId = commitId;
                }

                foreach (var rawStreamEntry in entries)
                {
                    //await statelessSession.InsertAsync(rawStreamEntry); // Async variant has some serious performance penalties (20% slower!)
                    statelessSession.Insert(rawStreamEntry);
                }

                result = await GetCurrentEventStoreCheckpointNumberInternalAsync(statelessSession);

                transaction.Commit();
            }

            return result;
        }

        protected async Task<int> GetCurrentEventStoreCheckpointNumberInternalAsync(IStatelessSession session)
        {
            var list = await session.Query<RawStreamEntry>()
                .Select(x => x.CheckpointNumber)
                .OrderByDescending(x => x)
                .Take(1)
                .ToListAsync();

            if (list.Count == 0)
            {
                return CheckpointDefaults.NoCheckpoint;
            }

            return list[0];
        }

        public async Task<int> GetCurrentEventStoreCheckpointNumberAsync()
        {
            var result = CheckpointDefaults.NoCheckpoint;

            using (var statelessSession = sessionFactory.OpenStatelessSession())
            using (var transaction = statelessSession.BeginTransaction())
            {
                try
                {
                    result = await GetCurrentEventStoreCheckpointNumberInternalAsync(statelessSession);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }

                transaction.Commit();
                return result;
            }
        }

        private List<string> GetPayloadValues(Type[] payloadTypes)
        {
            if (payloadTypes != null &&
                payloadTypes.Length > 0)
            {
                return payloadTypes.Select(x => Serializer.Binder.BindToName(x)).ToList();
            }

            return null;
        }
    }
}
