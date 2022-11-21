using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace SimpleEventSourcing.NHibernate.Tests.Storage
{
    public class TestEntityBMapping : ClassMapping<TestEntityB>
    {
        public TestEntityBMapping()
        {
            Table("TestEntityB");

            Id(x => x.Id, x => x.Generator(Generators.HighLow));
            Property(x => x.Value);
            Property(x => x.Streamname);
        }
    }
}
