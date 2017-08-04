using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class CheckpointInfoMapping : ClassMapping<CheckpointInfo>
    {
        public CheckpointInfoMapping()
        {
            Id(x => x.StateModel, c => c.Generator(Generators.Assigned));
            Property(x => x.CheckpointNumber);
        }
    }
}
