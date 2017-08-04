using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IPersistenceEngine
    {
        Task InitializeAsync();

        IEnumerable<IRawStreamEntry> LoadStreamEntriesByStream(string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);
        IEnumerable<IRawStreamEntry> LoadStreamEntriesByStream(string group, string category, string streamName, int minRevision = 0, int maxRevision = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);

        IEnumerable<IRawStreamEntry> LoadStreamEntries(int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);
        IEnumerable<IRawStreamEntry> LoadStreamEntries(string group, string category, int minCheckpointNumber = 0, int maxCheckpointNumber = int.MaxValue, Type[] payloadTypes = null, bool ascending = true, int take = int.MaxValue);

        int GetCurrentEventStoreCheckpointNumber();
        int SaveStreamEntries(IEnumerable<IRawStreamEntry> entries);

        ISerializer Serializer { get; }
    }
}
