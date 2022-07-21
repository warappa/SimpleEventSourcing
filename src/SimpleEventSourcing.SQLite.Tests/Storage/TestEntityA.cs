using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SimpleEventSourcing.SQLite.WriteModel.Tests
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
        ITestEntityASubEntity ITestEntityA.SubEntity { get => SubEntity; }
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

    public class SubData
    {
        public string PropA { get; set; }
        public string PropB { get; set; }
    }
}
