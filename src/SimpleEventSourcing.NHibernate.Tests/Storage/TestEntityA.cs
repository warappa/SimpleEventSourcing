using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    public class TestEntityA : ITestEntityA
    {
        object IReadModelBase.Id { get; set; } = 0;

        public virtual int Id { get => (int)(this as IReadModelBase).Id; set => (this as IReadModelBase).Id = value; }

        public virtual string Value { get; set; }
        public virtual string Streamname { get; set; }
        public virtual TestEntityASubEntity SubEntity { get; set; } = new TestEntityASubEntity
        {
            SubValue = "sub value"
        };
        ITestEntityASubEntity ITestEntityA.SubEntity { get => SubEntity; }
    }

    public class TestEntityASubEntity : ITestEntityASubEntity
    {
        public virtual int Id { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        public virtual string SubValue { get; set; }
    }
}
