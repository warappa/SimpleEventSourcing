using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Tests.ReadModel;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace SimpleEventSourcing.SQLite.Tests.Storage
{
    [Table("TestEntityA")]
    public class TestEntityA : ITestEntityA
    {
        [Ignore]
        object IReadModelBase.Id { get; set; } = 0;

        [PrimaryKey]
        [Column("Id")]
        public int Id { get => (int)(this as IReadModelBase).Id; set => (this as IReadModelBase).Id = value; }

        public string Value { get; set; }
        public string Streamname { get; set; }

        [TextBlob("SubDataSerialized")]
        public SubData SubData { get; set; }

        public string SubDataSerialized { get; set; }

        [ForeignKey(typeof(TestEntityASubEntity))]
        public int SubEntityId { get; set; }
        [ManyToOne]
        public TestEntityASubEntity SubEntity { get; set; } = new TestEntityASubEntity
        {
            SubValue = "sub value"
        };
        ITestEntityASubEntity ITestEntityA.SubEntity => SubEntity;
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public virtual List<TestEntityASubItem> SubItems { get; set; } = new List<TestEntityASubItem>();
    }

    [Table("TestEntityASubEntity")]
    public class TestEntityASubEntity : ITestEntityASubEntity
    {
        [PrimaryKey]
        [Column("Id")]
        public int Id { get; set; }
        [Ignore]
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        public string SubValue { get; set; }
    }

    [Table("TestEntityASubItem")]
    public class TestEntityASubItem : ITestEntityASubItem
    {
        [PrimaryKey]
        [Column("Id")]
        public int Id { get; set; }
        [Ignore]
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        [ForeignKey(typeof(TestEntityA))]
        public int ParentId { get; set; }

        public string SubItemValue { get; set; }
    }

    public class SubData
    {
        public string PropA { get; set; }
        public string PropB { get; set; }
    }
}
