using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
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
