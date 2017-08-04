using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.NHibernate.ReadModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SimpleEventSourcing.WriteModel;

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
            this.sessionFactory = sessionFactory;
            Serializer = serializer;
            this.configuration = configuration;
            this.batchSize = 1000;
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

            return Task.Delay(0);
        }

        public IEnumerable<IRawStreamEntry> LoadStreamEntriesByStream(string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesByStream(GroupConstants.All, null, streamName, minRevision, maxRevision, payloadTypes, ascending, take);
        }

        public IEnumerable<IRawStreamEntry> LoadStreamEntriesByStream(string group, string category, string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            List<RawStreamEntry> rawStreamEntries = null;

            using (var scope = OpenScope())
            using (var statelessSession = sessionFactory.OpenStatelessSession())
            using (var transaction = statelessSession.BeginTransaction())
            {
                try
                {
                    var query = statelessSession.Query<RawStreamEntry>();

                    query = query.Where(x => x.StreamRevision >= minRevision && x.StreamRevision <= maxRevision);

                    if (!string.IsNullOrWhiteSpace(streamName))
                    {
                        query = query.Where(x => x.StreamName == streamName);
                    }

                    if (payloadTypes != null &&
                        payloadTypes.Length > 0)
                    {
                        var payloadValues = payloadTypes.Select(x => Serializer.Binder.BindToName(x)).ToList();
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

                    query = query.Take(take);

                    rawStreamEntries = query.ToList();

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
                finally
                {
                    // TODO: error handling
                }

                transaction.Commit();
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

        public IEnumerable<IRawStreamEntry> LoadStreamEntries(int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntries(GroupConstants.All, null, minCheckpointNumber, maxCheckpointNumber, payloadTypes, ascending, take);
        }

        public IEnumerable<IRawStreamEntry> LoadStreamEntries(string group, string category, int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;

            //using (var scope = OpenScope())
            using (var statelessSession = sessionFactory.OpenStatelessSession())
            using (var transaction = statelessSession.BeginTransaction())
            {
                while (true)
                {
                    var query = statelessSession.Query<RawStreamEntry>();

                    query = query.Where(x => x.CheckpointNumber >= minCheckpointNumber && x.CheckpointNumber <= maxCheckpointNumber);

                    if (payloadTypes != null &&
                    payloadTypes.Length > 0)
                    {
                        var payloadValues = payloadTypes.Select(x => Serializer.Binder.BindToName(x)).ToList();
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

                    List<RawStreamEntry> rawStreamEntries = null;
                    try
                    {
                        rawStreamEntries = query.ToList();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());

                        throw;
                    }

                    if (rawStreamEntries.Count == 0)
                    {
                        transaction.Commit();

                        yield break;
                    }

                    foreach (var streamEntry in rawStreamEntries)
                    {
                        yield return streamEntry;
                    }

                    taken += rawStreamEntries.Count;

                    if (taken >= take)
                    {
                        transaction.Commit();

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
        }

        public int SaveStreamEntries(IEnumerable<IRawStreamEntry> entries)
        {
            int result;

            //using (var scope = OpenScope())
            using (var statelessSession = sessionFactory.OpenStatelessSession())
            using (var transaction = statelessSession.BeginTransaction())
            {
                statelessSession.SetBatchSize(100);

                var commitId = Guid.NewGuid().ToString();

                foreach (var rawStreamEntry in entries)
                {
                    rawStreamEntry.CommitId = commitId;
                }

                foreach (var rawStreamEntry in entries)
                {
                    statelessSession.Insert((RawStreamEntry)rawStreamEntry);
                }

                result = GetCurrentEventStoreCheckpointNumberInternal(statelessSession);

                transaction.Commit();
            }

            return result;
        }

        public static async Task RetryHelper(Action action)
        {
            await RetryHelper(() =>
            {
                action();
                return 0;
            }).ConfigureAwait(false);
        }

        public static async Task<T> RetryHelper<T>(Func<T> action)
        {
            var retryCount = 10;

            var ret = default(T);
            do
            {
                try
                {
                    ret = action();
                    break;
                }
                catch (AggregateException)
                {
                    retryCount--;
                    if (retryCount == 0)
                        throw;

                    await Task.Delay(100).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    retryCount--;
                    if (retryCount == 0)
                        throw;

                    await Task.Delay(100).ConfigureAwait(false);
                }
            } while (true);

            return ret;
        }

        protected int GetCurrentEventStoreCheckpointNumberInternal(IStatelessSession session)
        {
            var list = session.Query<RawStreamEntry>()
                .Select(x => x.CheckpointNumber)
                .OrderByDescending(x => x)
                .Take(1)
                .ToList();

            if (list.Count == 0)
            {
                return -1;
            }

            return list[0];
        }

        public int GetCurrentEventStoreCheckpointNumber()
        {
            int result = -1;

            using (var statelessSession = sessionFactory.OpenStatelessSession())
            using (var transaction = statelessSession.BeginTransaction())
            {
                try
                {
                    result = GetCurrentEventStoreCheckpointNumberInternal(statelessSession);
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
    }
}
