using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using SimpleEventSourcing.ReadModel;
using SQLite.Net.Attributes;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    [Table("PersistentEntities")]
    public class PersistentEntity : IStreamReadModel, IReadModel<string>
    {
        [PrimaryKey]
        public virtual string Id { get; set; }

        object IReadModelBase.Id { get { return Id; } set { Id = (string)value; } }

        public virtual string Streamname { get => Id; set => Id = value; }// { get { return Id.ToString(); } set { Id = int.Parse(value); } }

        public virtual string Name { get; set; }
    }

    public class PersistentEntityMap : ClassMapping<PersistentEntity>
    {
        public PersistentEntityMap()
        {
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