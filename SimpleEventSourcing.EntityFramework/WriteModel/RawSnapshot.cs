using SimpleEventSourcing.WriteModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFramework.WriteModel
{
    [Table("Snapshots")]
    public class RawSnapshot : IRawSnapshot
    {
        [Key]
        public int Id { get; set; }

        [Column("StreamName", TypeName = "nvarchar")]
        [StringLength(100)]
        [Index("LoadSnapshots", Order = 1, IsUnique = true)]
        public string StreamName { get; set; }

        [Column("StateIdentifier", TypeName = "nvarchar")]
        [StringLength(100)]
        [Index("LoadSnapshots", Order = 2, IsUnique = true)]
        public string StateIdentifier { get; set; }

        [Column("StreamRevision")]
        public int StreamRevision { get; set; }

        [Column("StateSerialized", TypeName = "nvarchar")]
        //[StringLength(4000)]
        public string StateSerialized { get; set; }

        [NotMapped]
        public DateTime CreatedAt { get { return CreatedAt2; } set { CreatedAt2 = value; } }

        // workaround EF datetime precision problem
        [Column("CreatedAt", TypeName = "datetime2")]
        public DateTime CreatedAt2 { get; set; }
    }
}
