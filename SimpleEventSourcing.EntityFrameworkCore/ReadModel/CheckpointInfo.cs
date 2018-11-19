using SimpleEventSourcing.ReadModel;
using System.ComponentModel.DataAnnotations;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public class CheckpointInfo : IReadModel<string>, ICheckpointInfo
    {
        [Key]
        public string StateModel { get; set; }
        public int CheckpointNumber { get; set; }

        object IReadModelBase.Id { get => StateModel; set => StateModel = (string)value; }
        string IReadModel<string>.Id { get => StateModel; set => StateModel = value; }
    }
}
