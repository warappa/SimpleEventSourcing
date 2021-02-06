using SimpleEventSourcing.WriteModel;
using System;

namespace SimpleEventSourcing.NHibernate.WriteModel
{
    public class RawStreamEntry : IRawStreamEntry
    {
        public virtual string StreamName { get; set; }
        public virtual string CommitId { get; set; }
        public virtual string MessageId { get; set; }
        public virtual int StreamRevision { get; set; }
        public virtual string PayloadType { get; set; }
        public virtual string Payload { get; set; }
        public virtual string Group { get; set; }
        public virtual string Category { get; set; }
        public virtual string Headers { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual int CheckpointNumber { get; set; }
    }
}
