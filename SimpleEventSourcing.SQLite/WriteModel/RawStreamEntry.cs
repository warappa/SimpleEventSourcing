using SimpleEventSourcing.WriteModel;
using SQLite.Net.Attributes;
using System;

namespace SimpleEventSourcing.SQLite.WriteModel
{
    [Table("Commits")]
    public class RawStreamEntry : IRawStreamEntry
    {
        [Column("StreamName")]
        [Indexed(Name = "LoadStreamMessages", Order = 2, Unique = true)]
        public string StreamName { get; set; }

        [Column("CommitId")]
        public string CommitId { get; set; }

        [Column("MessageId")]
        public string MessageId { get; set; }

        [Column("StreamRevision")]
        public int StreamRevision { get; set; }

        [Column("PayloadType")]
        [Indexed(Name = "LoadStreamMessages", Order = 3, Unique = true)]
        public string PayloadType { get; set; }

        [Column("Payload")]
        public string Payload { get; set; }

        [Column("Group")]
        public string Group { get; set; }

        [Column("Category")]
        public string Category { get; set; }

        [Column("Headers")]
        public string Headers { get; set; }

        [Column("DateTime")]
        public DateTime DateTime { get; set; }

        [Indexed(Name = "LoadStreamMessages", Order = 1, Unique = true)]
        [AutoIncrement]
        [PrimaryKey]
        public int CheckpointNumber { get; set; }
    }
}
