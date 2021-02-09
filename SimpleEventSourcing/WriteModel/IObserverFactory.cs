using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IObserverFactory
    {
        Task<IObserveRawStreamEntries> CreateObserverAsync(int lastKnownCheckpointNumber = -1, Type[] payloadTypes = null);
    }
}
