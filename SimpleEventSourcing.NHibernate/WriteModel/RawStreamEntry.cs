using SimpleEventSourcing.WriteModel;
using System;

namespace SimpleEventSourcing.NHibernate.WriteModel
{
    public class RawStreamEntry : IRawStreamEntry
    {
        virtual public string StreamName { get; set; }
        virtual public string CommitId { get; set; }
        virtual public string MessageId { get; set; }
        virtual public int StreamRevision { get; set; }
        virtual public string PayloadType { get; set; }
        virtual public string Payload { get; set; }
        virtual public string Group { get; set; }
        virtual public string Category { get; set; }
        virtual public string Headers { get; set; }
        virtual public DateTime DateTime { get; set; }
        virtual public int CheckpointNumber { get; set; }
    }
}
