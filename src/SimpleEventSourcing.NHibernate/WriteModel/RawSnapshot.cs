using SimpleEventSourcing.WriteModel;
using System;

namespace SimpleEventSourcing.NHibernate.WriteModel
{
    public class RawSnapshot : IRawSnapshot
    {
        public virtual int Id { get; set; }
        public virtual string StreamName { get; set; }
        public virtual string StateIdentifier { get; set; }
        public virtual int StreamRevision { get; set; }
        public virtual string StateSerialized { get; set; }
        public virtual DateTime CreatedAt { get; set; }
    }
}
