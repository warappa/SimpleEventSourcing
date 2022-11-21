using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Tests.ReadModel;
using SQLite;

namespace SimpleEventSourcing.SQLite.Tests.Storage
{
    [Table("TestEntityB")]
    public class TestEntityB : ITestEntityB
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
