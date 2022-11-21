using SimpleEventSourcing.ReadModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.Benchmarking.ReadModel
{
    [Table("PersistentEntities")]
    [global::SQLite.Table("PersistentEntities")]
    public class PersistentEntity : IStreamReadModel, IReadModel<string>
    {
        [global::SQLite.PrimaryKey]
        public virtual string? Id { get; set; }

        object IReadModelBase.Id { get => Id; set => Id = (string)value; }

        public virtual string Streamname { get => Id; set => Id = value; }// { get { return Id.ToString(); } set { Id = int.Parse(value); } }

        public virtual string? Name { get; set; }
    }
}
