using EntityFramework.BulkInsert.Extensions;
using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleEventSourcing.EntityFramework.WriteModel
{
    public class PersistenceEngine<TDbContext> : IPersistenceEngine
        where TDbContext : DbContext, IDbContext
    {
        public const string PayloadTypesSeparator = "~";
        private readonly IDbContextScopeFactory dbContextScopeFactory;
        private readonly int batchSize;

        public ISerializer Serializer { get; }

        public PersistenceEngine(IDbContextScopeFactory dbContextScopeFactory, ISerializer serializer)
            : this(dbContextScopeFactory, serializer, 1000)
        {
        }

        public PersistenceEngine(IDbContextScopeFactory dbContextScopeFactory, ISerializer serializer, int batchSize)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
            Serializer = serializer;
            this.batchSize = batchSize;
        }

        public async Task InitializeAsync()
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                try
                {
                    var dbContext = scope.DbContexts.Get<TDbContext>();
                    if (!CheckTableExists<RawStreamEntry>(dbContext))
                    {
                        var script = dbContext.ObjectContext.CreateDatabaseScript();
                        var steps = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var step in steps)
                        {
                            await dbContext.Database.ExecuteSqlCommandAsync(step).ConfigureAwait(false);
                        }
                    }

                    await scope.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }
            }
        }

        private static bool CheckTableExists<T>(DbContext dbContext) where T : class
        {
            try
            {
                dbContext.Set<T>().Count();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
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
                using (var transaction = new AsyncTransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.RepeatableRead
                }))
                using (var scope = dbContextScopeFactory.Create())
                {
                    var dbContext = scope.DbContexts.Get<TDbContext>();

                    try
                    {
                        IQueryable<RawStreamEntry> query = dbContext.Set<RawStreamEntry>().AsNoTracking();

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

                        rawStreamEntries = await query.ToListAsync().ConfigureAwait(false);
                    }
                    finally
                    {
                        transaction.Complete();
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
                maxCheckpointNumber = await GetCurrentEventStoreCheckpointNumberAsync().ConfigureAwait(false);
            }

            while (true)
            {
                using (var transaction = new AsyncTransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.RepeatableRead
                }))
                using (var scope = dbContextScopeFactory.Create())
                {
                    var dbContext = scope.DbContexts.Get<TDbContext>();

                    IQueryable<RawStreamEntry> query = dbContext.Set<RawStreamEntry>().AsNoTracking();

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
                        rawStreamEntries = await query.ToListAsync().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());

                        throw;
                    }

                    transaction.Complete();
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

        public async Task<int> SaveStreamEntriesAsync(IEnumerable<IRawStreamEntry> rawStreamEntries)
        {
            int result;

            using (var transaction = new AsyncTransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }))
            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                var commitId = Guid.NewGuid().ToString();

                foreach (var rawStreamEntry in rawStreamEntries)
                {
                    rawStreamEntry.CommitId = commitId;
                }

                dbContext.BulkInsert(rawStreamEntries.Cast<RawStreamEntry>());

                result = await GetCurrentEventStoreCheckpointNumberInternalAsync(dbContext).ConfigureAwait(false);

                transaction.Complete();
            }

            return result;
        }

        protected async Task<int> GetCurrentEventStoreCheckpointNumberInternalAsync(DbContext dbContext)
        {
            var list = dbContext.Set<RawStreamEntry>()
                .Local
                .Select(x => x.CheckpointNumber)
                .OrderByDescending(x => x)
                .Take(1)
                .ToList();

            if (list.Count == 0)
            {
                list = await dbContext.Set<RawStreamEntry>()
                    .AsNoTracking()
                    .Select(x => x.CheckpointNumber)
                    .OrderByDescending(x => x)
                    .Take(1)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            if (list.Count == 0)
            {
                return CheckpointDefaults.NoCheckpoint;
            }

            return list[0];
        }

        public async Task<int> GetCurrentEventStoreCheckpointNumberAsync()
        {
            int result;

            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                try
                {
                    result = await GetCurrentEventStoreCheckpointNumberInternalAsync(dbContext).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }

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

        public async Task<IRawSnapshot> LoadLatestSnapshotAsync(string streamName, string stateIdentifier, int maxRevision = int.MaxValue)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                try
                {
                    return dbContext.Set<RawSnapshot>()
                        .Where(x => 
                            x.StreamName == streamName &&
                            x.StateIdentifier == stateIdentifier &&
                            x.StreamRevision <= maxRevision)
                        .OrderByDescending(x => x.StreamRevision)
                        .FirstOrDefault();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }
            }
        }

        public async Task SaveSnapshotAsync(IStreamState state, int streamRevision)
        {
            await SaveSnapshot(new RawSnapshot
            {
                StreamName = state.StreamName,
                StateIdentifier = Serializer.Binder.BindToName(state.GetType()),
                StreamRevision = streamRevision,
                StateSerialized = Serializer.Serialize(state),
                CreatedAt = DateTime.UtcNow
            }).ConfigureAwait(false);
        }

        private async Task SaveSnapshot(RawSnapshot snapshot)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                try
                {
                    dbContext.Set<RawSnapshot>()
                        .Add(snapshot);
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }
            }
        }
    }
}
