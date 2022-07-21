using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.ReadModel.InMemory.Tests
{
    public class TestEntityA : ITestEntityA
    {
        object IReadModelBase.Id { get; set; } = 0;

        public virtual int Id { get => (int)(this as IReadModelBase).Id; set => (this as IReadModelBase).Id = value; }

        public virtual string Value { get; set; }
        public string Streamname { get; set; }
        public TestEntityASubEntity SubEntity { get; } = new TestEntityASubEntity
        {
            SubValue = "sub value"
        };
        ITestEntityASubEntity ITestEntityA.SubEntity { get => SubEntity; }
    }

    public class TestEntityASubEntity : ITestEntityASubEntity
    {
        public int Id { get; set; }
        public string SubValue { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
    }
}
