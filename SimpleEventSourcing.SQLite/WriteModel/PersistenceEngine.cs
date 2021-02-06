﻿using SimpleEventSourcing.WriteModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.WriteModel
{
    public class PersistenceEngine : IPersistenceEngine
    {
        public const string PayloadTypesSeparator = "~";

        public ISerializer Serializer { get; }

        private readonly Func<SQLiteConnectionWithLock> connectionFactory;
        private readonly int batchSize;

        public PersistenceEngine(Func<SQLiteConnectionWithLock> connectionFactory, ISerializer serializer)
            : this(connectionFactory, serializer, 1000)
        {
        }

        public PersistenceEngine(Func<SQLiteConnectionWithLock> connectionFactory, ISerializer serializer, int batchSize)
        {
            this.connectionFactory = connectionFactory;
            Serializer = serializer;
            this.batchSize = batchSize;
        }

        public Task InitializeAsync()
        {
            var connection = connectionFactory();

            using (connection.Lock())
            {
                try
                {
                    connection.CreateTable<RawStreamEntry>();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }
            }

            return Task.Delay(0);
        }


        public IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesByStreamAsync(GroupConstants.All, null, streamName, minRevision, maxRevision, payloadTypes, ascending, take);
        }

        public async IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string group, string category, string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            var conn = connectionFactory();
            List<RawStreamEntry> rawStreamEntries = null;

            using (conn.Lock())
            {
                try
                {
                    var cmd = conn.CreateCommand("");

                    var commandText = new StringBuilder();
                    commandText.Append(@"select streamName, commitId, messageId, streamRevision, payloadType, payload, headers, [group], category, checkpointNumber, dateTime from commits
where
streamRevision >= @minRevision and streamRevision <= @maxRevision ");

                    if (!string.IsNullOrWhiteSpace(streamName))
                    {
                        commandText.Append(@" and streamName = @streamName ");
                    }

                    if (payloadTypes != null &&
                        payloadTypes.Length > 0)
                    {
                        commandText.Append(" and (");
                        for (var i = 0; i < payloadTypes.Length; i++)
                        {
                            commandText.Append(
                                $@"payloadType = '{Serializer.Binder.BindToName(payloadTypes[i])}' ");
                            if (i < payloadTypes.Length - 1)
                            {
                                commandText.Append(" or ");
                            }
                        }
                        commandText.Append(") ");
                    }

                    if (group != null &&
                        group != GroupConstants.All)
                    {
                        commandText.Append(" and [group]=@group");

                        cmd.Bind("@group", group);
                    }

                    if (category != null)
                    {
                        commandText.Append(" and category=@category");

                        cmd.Bind("@category", category);
                    }

                    commandText.Append(@" order by checkpointNumber " + (ascending ? "asc" : "desc"));

                    var nextBatchSize = Math.Min(take - taken, batchSize);

                    commandText.Append($" limit {nextBatchSize}");

                    cmd.CommandText = commandText.ToString();

                    if (!string.IsNullOrWhiteSpace(streamName))
                    {
                        cmd.Bind("@streamName", streamName);
                    }
                    cmd.Bind("@minRevision", minRevision);
                    cmd.Bind("@maxRevision", maxRevision);

                    rawStreamEntries = cmd.ExecuteQuery<RawStreamEntry>();

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
                        minRevision = rawStreamEntries
                            [rawStreamEntries.Count - 1]
                            .StreamRevision + 1;
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
        }

        public IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            return LoadStreamEntriesAsync(GroupConstants.All, null, minCheckpointNumber, maxCheckpointNumber, payloadTypes, ascending, take);
        }

        public async IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(string group, string category, int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue)
        {
            var taken = 0;
            string payloadPredicate = null;

            var connection = connectionFactory();

            if (maxCheckpointNumber == int.MaxValue)
            {
                maxCheckpointNumber = await GetCurrentEventStoreCheckpointNumberAsync();
            }

            if (payloadTypes != null &&
                payloadTypes.Length > 0)
            {
                var commandText = new StringBuilder();
                commandText.Append(" and (");

                for (var i = 0; i < payloadTypes.Length; i++)
                {
                    commandText.Append($@"payloadType = '{Serializer.Binder.BindToName(payloadTypes[i])}' ");

                    if (i < payloadTypes.Length - 1)
                    {
                        commandText.Append(" or ");
                    }
                }

                commandText.Append(") ");

                payloadPredicate = commandText.ToString();
            }

            using (connection.Lock())
            {
                var cmd = connection.CreateCommand("");

                while (true)
                {
                    var commandText = new StringBuilder();
                    commandText.Append(@"select streamName, commitId, messageId, streamRevision, payloadType, payload, headers, checkpointNumber, [group], category, dateTime from commits
where checkpointNumber >= @minCheckpointNumber and checkpointNumber <= @maxCheckpointNumber ");

                    if (payloadPredicate is string)
                    {
                        commandText.Append(payloadPredicate);
                    }

                    if (group != null &&
                        group != GroupConstants.All)
                    {
                        commandText.Append(" and [group]=@group");

                        cmd.Bind("@group", group);
                    }

                    if (category != null)
                    {
                        commandText.Append(" and category=@category");

                        cmd.Bind("@category", category);
                    }

                    commandText.Append($" order by checkpointNumber {(ascending ? "asc" : "desc")}");

                    var nextBatchSize = Math.Min(take - taken, batchSize);

                    commandText.Append($" limit {nextBatchSize}");

                    cmd.Bind("@minCheckpointNumber", minCheckpointNumber);
                    cmd.Bind("@maxCheckpointNumber", maxCheckpointNumber);

                    cmd.CommandText = commandText.ToString();

                    List<RawStreamEntry> rawStreamEntries = null;
                    try
                    {
                        rawStreamEntries = cmd.ExecuteQuery<RawStreamEntry>().ToList();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                        connection.Rollback();
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

        public async Task<int> SaveStreamEntriesAsync(IEnumerable<IRawStreamEntry> entries)
        {
            var connection = connectionFactory();
            int result;

            using (connection.Lock())
            {
                connection.InsertAll(entries);
                result = await GetCurrentEventStoreCheckpointNumberInternalAsync(connection);
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

            T ret;
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
                catch (SQLiteException)
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

        protected async Task<int> GetCurrentEventStoreCheckpointNumberInternalAsync(SQLiteConnection connection)
        {
            try
            {
                var cmd = connection.CreateCommand("select checkpointNumber from commits order by checkpointNumber desc limit 1");
                var res = cmd.ExecuteScalar<int>();
                if (res == 0)
                {
                    return -1;
                }

                return res;
            }
            catch
            {
                // blank
            }
            return -1;
        }

        public async Task<int> GetCurrentEventStoreCheckpointNumberAsync()
        {
            var connection = connectionFactory();
            using (connection.Lock())
            {
                try
                {
                    return await GetCurrentEventStoreCheckpointNumberInternalAsync(connection);
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
