using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System.Collections.Generic;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    public class TestEntityAMapping : ClassMapping<TestEntityA>
    {
        public TestEntityAMapping()
        {
            Table("TestEntityA");

            Id(x => x.Id, x => x.Generator(Generators.HighLow)); //, y => y.Params(new { table = "TestEntityAHiLo" })));
            Property(x => x.Value);
            Property(x => x.Streamname);
        }
    }
}
