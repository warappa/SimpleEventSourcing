using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class CheckpointInfo : IReadModel<string>, ICheckpointInfo
    {
        public virtual string StateModel { get; set; }
        public virtual int CheckpointNumber { get; set; }

        object IReadModelBase.Id { get { return StateModel; } set { StateModel = (string)value; } }
        string IReadModel<string>.Id { get => StateModel; set => StateModel = value; }
    }
}
