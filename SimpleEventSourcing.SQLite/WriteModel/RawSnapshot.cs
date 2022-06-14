using SimpleEventSourcing.WriteModel;
using SQLite;
using System;

namespace SimpleEventSourcing.SQLite.WriteModel
{
    [Table("Snapshots")]
    public class RawSnapshot : IRawSnapshot
    {
        [AutoIncrement]
        [PrimaryKey]
        public int Id { get; set; }

        [Column("StreamName")]
        [Indexed(Name = "LoadSnapshots", Order = 1, Unique = true)]
        public string StreamName { get; set; }

        [Column("StateIdentifier")]
        [Indexed(Name = "LoadSnapshots", Order = 2, Unique = true)]
        public string StateIdentifier { get; set; }

        [Column("StreamRevision")]
        [Indexed(Name = "LoadSnapshots", Order = 3, Unique = true)]
        public int StreamRevision { get; set; }

        [Column("StateSerialized")]
        public string StateSerialized { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
