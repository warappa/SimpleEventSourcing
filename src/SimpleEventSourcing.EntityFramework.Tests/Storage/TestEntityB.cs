using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Tests.ReadModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFramework.Tests.Storage
{
    [Table("TestEntityB")]
    public class TestEntityB : ITestEntityB
    {
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }

        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public string Value { get; set; }
        public string Streamname { get; set; }
    }
}
