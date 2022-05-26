using SimpleEventSourcing.ReadModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFramework.ReadModel
{
    [Table("CheckpointInfos")]
    public class CheckpointInfo : IReadModel<string>, ICheckpointInfo
    {
        [Key]
        public string ProjectorIdentifier { get; set; }
        public int CheckpointNumber { get; set; }

        object IReadModelBase.Id { get => ProjectorIdentifier; set => ProjectorIdentifier = (string)value; }
        string IReadModel<string>.Id { get => ProjectorIdentifier; set => ProjectorIdentifier = value; }
    }
}
