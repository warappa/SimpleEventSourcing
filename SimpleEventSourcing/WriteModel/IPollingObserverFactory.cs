using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IPollingObserverFactory : IObserverFactory
    {
        Task<IObserveRawStreamEntries> CreateObserverAsync(TimeSpan interval, int lastKnownCheckpointNumber = CheckpointDefaults.NoCheckpoint, Type[] payloadTypes = null);
    }
}
