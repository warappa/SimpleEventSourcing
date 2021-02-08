using SimpleEventSourcing.ReadModel;
using SQLite;

namespace SimpleEventSourcing.SQLite.ReadModel
{
    [Table("CheckpointInfos")]
    public class CheckpointInfo : IReadModel<string>, ICheckpointInfo
    {
        [PrimaryKey]
        public string StateModel { get; set; }
        [Indexed]
        public int CheckpointNumber { get; set; }

        object IReadModelBase.Id { get { return StateModel; } set { StateModel = (string)value; } }
        string IReadModel<string>.Id { get => StateModel; set => StateModel = value; }
    }
}
