using System;

namespace SimpleEventSourcing.WriteModel
{
    public interface IRawSnapshot
    {
        string StreamName { get; }
        string StateIdentifier { get; }
        int StreamRevision { get; }
        string StateSerialized { get; }
        DateTime CreatedAt { get; }
    }
}

