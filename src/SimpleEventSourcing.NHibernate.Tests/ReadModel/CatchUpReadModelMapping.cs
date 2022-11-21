using NHibernate.Mapping.ByCode.Conformist;
using SimpleEventSourcing.Tests.ReadModel;

namespace SimpleEventSourcing.NHibernate.Tests.ReadModel
{
    public class CatchUpReadModelMapping : ClassMapping<CatchUpReadModel>
    {
        public CatchUpReadModelMapping()
        {
            Table("CatchUpReadModel");

            Id(x => x.Id);
            Property(x => x.Count, cfg => { });
        }
    }
}
