using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.EntityFramework.WriteModel.Tests
{
    [Table("TestEntityA")]
    public class TestEntityA : ITestEntityA
    {
        object IReadModelBase.Id
        {
            get => Id;
            set => Id = (int)value;
        }

        [Key]
        [Column("Id")]
        public int Id
        {
            get;
            set;
        }

        public string Value { get; set; }
        public string Streamname { get; set; }

        public TestEntityASubEntity SubEntity { get; set; } = new TestEntityASubEntity
        {
            SubValue = "sub value"
        };
        ITestEntityASubEntity ITestEntityA.SubEntity { get => SubEntity; }

        public virtual ICollection<TestEntityASubItem> SubItems { get; set; } = new List<TestEntityASubItem>();

        public override bool Equals(object obj)
        {
            var other = obj as TestEntityA;

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    [Table("TestEntityASubEntity")]
    public class TestEntityASubEntity : ITestEntityASubEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }

        public string SubValue { get; set; }
    }

    [Table("TestEntityASubItem")]
    public class TestEntityASubItem : ITestEntityASubItem
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        public int ParentId { get; set; }

        public string SubItemValue { get; set; }
    }
}
