using System;

namespace SimpleEventSourcing.WriteModel.InMemory
{
    public class RawSnapshot : IRawSnapshot
    {
        public string StreamName { get; set; }
        public string StateIdentifier { get; set; }
        public int StreamRevision { get; set; }
        public string StateSerialized { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

