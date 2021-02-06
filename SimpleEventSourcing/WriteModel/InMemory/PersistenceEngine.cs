using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.InMemory
{
    public class PersistenceEngine : IPersistenceEngine
    {
        private readonly List<RawStreamEntry> streamEntries = new();
        private readonly int batchSize;
        private int checkpointNumber = 0;

        public ISerializer Serializer { get; private set; }

        public PersistenceEngine(ISerializer serializer)
        {
            Serializer = serializer;
            batchSize = 1000;
        }

        public PersistenceEngine(ISerializer serializer, int batchSize)
        {
            Serializer = serializer;
            this.batchSize = batchSize;
        }

        public async Task<int> GetCurrentEventStoreCheckpointNumberAsync()
        {
            var list = streamEntries
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

        public IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesAsync(GroupConstants.All, null, minCheckpointNumber, maxCheckpointNumber, payloadTypes, ascending, take);
        }

        public async IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(string group, string category, int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            List<string> payloadValues = null;

            if (maxCheckpointNumber == int.MaxValue)
            {
                maxCheckpointNumber = await GetCurrentEventStoreCheckpointNumberAsync();
            }

            if (payloadTypes != null &&
                payloadTypes.Length > 0)
            {
                payloadValues = payloadTypes.Select(x => Serializer.Binder.BindToName(x)).ToList();
            }

            while (true)
            {
                var query = streamEntries.AsQueryable();

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

        public IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesByStreamAsync(GroupConstants.All, null, streamName, minRevision, maxRevision, payloadTypes, ascending, take);
        }

        public async IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string group, string category, string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            List<RawStreamEntry> rawStreamEntries = null;
            List<string> payloadValues = null;

            if (payloadTypes != null &&
                payloadTypes.Length > 0)
            {
                payloadValues = payloadTypes.Select(x => Serializer.Binder.BindToName(x)).ToList();
            }

            try
            {
                var query = streamEntries.AsQueryable();

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
        }

        public async Task<int> SaveStreamEntriesAsync(IEnumerable<IRawStreamEntry> rawStreamEntries)
        {
            int result;

            var commitId = Guid.NewGuid().ToString();

            foreach (var rawStreamEntry in rawStreamEntries)
            {
                rawStreamEntry.CommitId = commitId;
                rawStreamEntry.CheckpointNumber = Interlocked.Add(ref checkpointNumber, 1);
            }

            streamEntries.AddRange(rawStreamEntries.Cast<RawStreamEntry>());

            result = await GetCurrentEventStoreCheckpointNumberAsync();

            return result;
        }

        public Task InitializeAsync()
        {
            return Task.Delay(0);
        }
    }
}
