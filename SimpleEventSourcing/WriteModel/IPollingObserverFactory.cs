using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IPollingObserverFactory : IObserverFactory
    {
        Task<IObserveRawStreamEntries> CreateObserverAsync(TimeSpan interval, int lastKnownCheckpointNumber = -1, Type[] payloadTypes = null);
    }
}
