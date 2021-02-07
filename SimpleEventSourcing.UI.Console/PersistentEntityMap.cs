using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    public class PersistentEntityMap : ClassMapping<PersistentEntity>
    {
        public PersistentEntityMap()
        {
            Table("PersistentEntities");

            Id(x => x.Id, config =>
            {
                //config.Generator(global::NHibernate.Mapping.ByCode.Generators.HighLow);
                config.Generator(global::NHibernate.Mapping.ByCode.Generators.Assigned);
            });
            Property(x => x.Streamname);
            Property(x => x.Name);
            Cache(x => {
                x.Usage(CacheUsage.NonstrictReadWrite);
                x.Include(CacheInclude.All);
            });
        }
    }
}