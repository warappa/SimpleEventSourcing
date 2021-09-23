using SimpleEventSourcing.ReadModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFramework.ReadModel
{
    [Table("CheckpointInfos")]
    public class CheckpointInfo : IReadModel<string>, ICheckpointInfo
    {
        [Key]
        public string StateModel { get; set; }
        public int CheckpointNumber { get; set; }

        object IReadModelBase.Id { get => StateModel; set => StateModel = (string)value; }
        string IReadModel<string>.Id { get => StateModel; set => StateModel = value; }
    }
}
