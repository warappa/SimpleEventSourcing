using System;

namespace SimpleEventSourcing.WriteModel
{
    public interface IPoller
    {
        IObserveRawStreamEntries ObserveFrom(int lastKnownCheckpointNumber = -1, Type[] payloadTypes = null);
    }
}
