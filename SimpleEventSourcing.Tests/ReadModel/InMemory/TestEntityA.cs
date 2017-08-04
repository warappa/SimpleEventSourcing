using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.ReadModel.InMemory.Tests
{
    public class TestEntityA : ITestEntityA
    {
        object IReadModelBase.Id { get; set; } = 0;

        virtual public int Id { get => (int)(this as IReadModelBase).Id; set => (this as IReadModelBase).Id = value; }

        virtual public string Value { get; set; }
        public string Streamname { get; set; }
    }
}
