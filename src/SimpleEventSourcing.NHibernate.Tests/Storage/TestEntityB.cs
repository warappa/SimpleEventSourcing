using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Tests.ReadModel;

namespace SimpleEventSourcing.NHibernate.Tests.Storage
{
    public class TestEntityB : ITestEntityB
    {
        object IReadModelBase.Id { get; set; } = 0;

        public virtual int Id { get => (int)(this as IReadModelBase).Id; set => (this as IReadModelBase).Id = value; }

        public virtual string Value { get; set; }
        public virtual string Streamname { get; set; }
    }
}
