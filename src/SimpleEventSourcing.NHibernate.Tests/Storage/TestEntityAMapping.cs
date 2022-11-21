using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace SimpleEventSourcing.NHibernate.Tests.Storage
{
    public class TestEntityAMapping : ClassMapping<TestEntityA>
    {
        public TestEntityAMapping()
        {
            Table("TestEntityA");

            Id(x => x.Id, x => x.Generator(Generators.HighLow)); //, y => y.Params(new { table = "TestEntityAHiLo" })));
            Property(x => x.Value);
            Property(x => x.Streamname);

            OneToOne(x => x.SubEntity, config =>
            {
            });
        }
    }

    public class TestEntityASubEntityMapping : ClassMapping<TestEntityASubEntity>
    {
        public TestEntityASubEntityMapping()
        {
            Table("TestEntityASubEntity");

            Id(x => x.Id, x => x.Generator(Generators.HighLow)); //, y => y.Params(new { table = "TestEntityAHiLo" })));
            Property(x => x.SubValue);
        }
    }
}
