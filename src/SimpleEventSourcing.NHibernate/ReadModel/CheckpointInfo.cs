using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class CheckpointInfo : IReadModel<string>, ICheckpointInfo
    {
        public virtual string ProjectorIdentifier { get; set; }
        public virtual int CheckpointNumber { get; set; }

        object IReadModelBase.Id { get { return ProjectorIdentifier; } set { ProjectorIdentifier = (string)value; } }
        string IReadModel<string>.Id { get => ProjectorIdentifier; set => ProjectorIdentifier = value; }
    }
}
