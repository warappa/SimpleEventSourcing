using System;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    public class TestEntityA : ITestEntityA
    {
        object IReadModelBase.Id { get; set; } = 0;

        virtual public int Id { get => (int)(this as IReadModelBase).Id; set => (this as IReadModelBase).Id = value; }

        virtual public string Value { get; set; }
        public virtual string Streamname { get; set; }
    }
}
