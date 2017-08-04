using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SQLite.Net.Attributes;

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
    }
}
