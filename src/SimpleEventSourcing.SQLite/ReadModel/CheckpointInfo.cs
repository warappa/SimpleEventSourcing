using SimpleEventSourcing.ReadModel;
using SQLite;

namespace SimpleEventSourcing.SQLite.ReadModel
{
    [Table("CheckpointInfos")]
    public class CheckpointInfo : IReadModel<string>, ICheckpointInfo
    {
        [PrimaryKey]
        public string ProjectorIdentifier { get; set; }
        [Indexed]
        public int CheckpointNumber { get; set; }

        object IReadModelBase.Id { get => ProjectorIdentifier; set => ProjectorIdentifier = (string)value; }
        string IReadModel<string>.Id { get => ProjectorIdentifier; set => ProjectorIdentifier = value; }
    }
}
