using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel
{
    public class PersistenceEngine<TDbContext> : IPersistenceEngine
        where TDbContext : DbContext
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
                        var script = dbContext.GetService<IRelationalDatabaseCreator>().GenerateCreateScript();
                        var steps = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var step in steps)
                        {
                            if (string.IsNullOrWhiteSpace(step))
                            {
                                continue;
                            }

                            dbContext.Database.ExecuteSqlCommand(new RawSqlString(step));
                        }
                    }

                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
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

        public IEnumerable<IRawStreamEntry> LoadStreamEntriesByStream(string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesByStream(GroupConstants.All, null, streamName, minRevision, maxRevision, payloadTypes, ascending, take);
        }

        public IEnumerable<IRawStreamEntry> LoadStreamEntriesByStream(string group, string category, string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            List<RawStreamEntry> rawStreamEntries = null;

            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                try
                {
                    var query = dbContext.Set<RawStreamEntry>().AsNoTracking();

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

                    var nextBatchSize = Math.Min(take - taken, batchSize);

                    query = query.Take(nextBatchSize);

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

                }
            }
        }

        public IEnumerable<IRawStreamEntry> LoadStreamEntries(int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntries(GroupConstants.All, null, minCheckpointNumber, maxCheckpointNumber, payloadTypes, ascending, take);
        }

        public IEnumerable<IRawStreamEntry> LoadStreamEntries(string group, string category, int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;

            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                if (maxCheckpointNumber == int.MaxValue)
                {
                    maxCheckpointNumber = GetCurrentEventStoreCheckpointNumber();
                }

                while (true)
                {
                    var query = dbContext.Set<RawStreamEntry>().AsNoTracking();

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
        }

        public int SaveStreamEntries(IEnumerable<IRawStreamEntry> rawStreamEntries)
        {
            int result;

            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                var commitId = Guid.NewGuid().ToString();

                foreach (var rawStreamEntry in rawStreamEntries)
                {
                    rawStreamEntry.CommitId = commitId;
                }

                dbContext.Set<RawStreamEntry>().AddRange(rawStreamEntries.Cast<RawStreamEntry>());

                var rowCount = 0;
                //scope.RefreshEntitiesInParentScope(rawStreamEntries);
                rowCount = scope.SaveChanges();
                //RetryHelper(() => rowCount = scope.SaveChanges()).Wait();

                if (rowCount == 0)
                {
                    //throw new Exception("Stream entries not inserted!");
                }

                result = GetCurrentEventStoreCheckpointNumberInternal(dbContext);
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
                    {
                        throw;
                    }

                    await Task.Delay(100).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        throw;
                    }

                    await Task.Delay(100).ConfigureAwait(false);
                }
            } while (true);

            return ret;
        }

        protected int GetCurrentEventStoreCheckpointNumberInternal(DbContext dbContext)
        {
            var list = dbContext.Set<RawStreamEntry>()
                .Local
                .Select(x => x.CheckpointNumber)
                .OrderByDescending(x => x)
                .Take(1)
                .ToList();
            if (list.Count == 0)
            {
                list = dbContext.Set<RawStreamEntry>()
                .AsNoTracking()
                .Select(x => x.CheckpointNumber)
                .OrderByDescending(x => x)
                .Take(1)
                .ToList();
            }

            if (list.Count == 0)
            {
                return -1;
            }

            return list[0];
        }

        public int GetCurrentEventStoreCheckpointNumber()
        {
            int result;

            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();

                try
                {
                    result = GetCurrentEventStoreCheckpointNumberInternal(dbContext);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }

                return result;
            }
        }
    }
}
