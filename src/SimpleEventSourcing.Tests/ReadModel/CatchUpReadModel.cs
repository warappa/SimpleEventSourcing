using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEventSourcing.ReadModel.Tests
{
    public class CatchUpReadModel : IReadModel<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        object IReadModelBase.Id { get => Id; set => Id = (int)value; }
        public virtual int Count { get; set; }
    }
}
