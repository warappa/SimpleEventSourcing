using SimpleEventSourcing.State;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IPersistenceEngine
    {
        Task InitializeAsync();

        IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);
        IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesByStreamAsync(string group, string category, string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);

        IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);
        IAsyncEnumerable<IRawStreamEntry> LoadStreamEntriesAsync(string group, string category, int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);

        Task<int> GetCurrentEventStoreCheckpointNumberAsync();
        Task<int> SaveStreamEntriesAsync(IEnumerable<IRawStreamEntry> entries);

        ISerializer Serializer { get; }

        Task<IRawSnapshot> LoadLatestSnapshotAsync(string streamName, string stateIdentifier);
        Task SaveSnapshot(IStreamState state, int streamRevision);
    }
}
