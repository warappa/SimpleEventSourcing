using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class CheckpointInfoMapping : ClassMapping<CheckpointInfo>
    {
        public CheckpointInfoMapping()
        {
            Table("CheckpointInfos");

            Id(x => x.ProjectorIdentifier, c => c.Generator(Generators.Assigned));
            Property(x => x.CheckpointNumber);
        }
    }
}
