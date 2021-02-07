using SimpleEventSourcing.ReadModel;
using SQLite;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    [Table("PersistentEntities")]
    public class PersistentEntity : IStreamReadModel, IReadModel<string>
    {
        [PrimaryKey]
        public virtual string Id { get; set; }

        object IReadModelBase.Id { get { return Id; } set { Id = (string)value; } }

        public virtual string Streamname { get => Id; set => Id = value; }// { get { return Id.ToString(); } set { Id = int.Parse(value); } }

        public virtual string Name { get; set; }
    }
}