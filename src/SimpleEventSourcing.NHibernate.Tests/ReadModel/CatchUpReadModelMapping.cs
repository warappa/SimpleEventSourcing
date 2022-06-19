using NHibernate.Mapping.ByCode.Conformist;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.NHibernate.ReadModel.Tests
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
