﻿using SimpleEventSourcing.EntityFrameworkCore.Internal;
using SimpleEventSourcing.WriteModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel
{
    [Table("Commits")]
    public class RawStreamEntry : IRawStreamEntry
    {
        [Column("StreamName", TypeName = "varchar(100)")]
        [StringLength(100)]
        [Index("LoadStreamMessages", Order = 2)]
        public string StreamName { get; set; }

        [Column("CommitId")]
        public string CommitId { get; set; }

        [Column("MessageId")]
        public string MessageId { get; set; }

        [Column("StreamRevision")]
        public int StreamRevision { get; set; }

        [Column("PayloadType", TypeName = "varchar(100)")]
        [StringLength(100)]
        [Index("LoadStreamMessages", Order = 3)]
        public string PayloadType { get; set; }

        [Column("Payload")]
        public string Payload { get; set; }

        [Column("Group")]
        [Index("LoadStreamMessages", Order = 4)]
        public string Group { get; set; }

        [Column("Category")]
        [Index("LoadStreamMessages", Order = 5)]
        public string Category { get; set; }

        [Column("Headers")]
        public string Headers { get; set; }

        [NotMapped]
        public DateTime DateTime { get => DateTime2.Value; set => DateTime2 = value; }

        // workaround EF datetime precision problem
        [Column("DateTime", TypeName = "datetime2")]
        public DateTime? DateTime2 { get; set; }

        [Index("LoadStreamMessages", Order = 1, IsUnique = true)]
        [Key]
        public int CheckpointNumber { get; set; }
    }
}
