using System;

namespace SimpleEventSourcing.WriteModel
{
    public interface IRawStreamEntry : IHasCheckpointNumber
    {
        string StreamName { get; set; }
        string CommitId { get; set; }
        string MessageId { get; set; }
        int StreamRevision { get; set; }
        string PayloadType { get; set; }
        string Payload { get; set; }
        string Headers { get; set; }
        string Group { get; set; }
        string Category { get; set; }
        DateTime DateTime { get; set; }
    }
}
