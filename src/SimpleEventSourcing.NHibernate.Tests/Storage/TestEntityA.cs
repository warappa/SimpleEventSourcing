using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

        public virtual ICollection<TestEntityASubItem> SubItems { get; set; } = new List<TestEntityASubItem>();
    }

    public class TestEntityASubEntity : ITestEntityASubEntity
    {
        public virtual int Id { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        public virtual string SubValue { get; set; }
    }

    public class TestEntityASubItem : ITestEntityASubItem
    {
        public int Id { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        public int ParentId { get; set; }

        public string SubItemValue { get; set; }
    }
}
