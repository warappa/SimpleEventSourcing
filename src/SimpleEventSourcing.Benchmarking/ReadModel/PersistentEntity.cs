using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.Benchmarking.ReadModel
{
    [System.ComponentModel.DataAnnotations.Schema.Table("PersistentEntities")]
    [global::SQLite.Table("PersistentEntities")]
    public class PersistentEntity : IStreamReadModel, IReadModel<string>
    {
        [global::SQLite.PrimaryKey]
        public virtual string Id { get; set; }

        object IReadModelBase.Id { get { return Id; } set { Id = (string)value; } }

        public virtual string Streamname { get => Id; set => Id = value; }// { get { return Id.ToString(); } set { Id = int.Parse(value); } }

        public virtual string Name { get; set; }
    }
}
