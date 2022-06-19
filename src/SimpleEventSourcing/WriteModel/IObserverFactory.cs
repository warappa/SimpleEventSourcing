using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IObserverFactory
    {
        Task<IObserveRawStreamEntries> CreateObserverAsync(int lastKnownCheckpointNumber = CheckpointDefaults.NoCheckpoint, Type[] payloadTypes = null);
    }
}
