using SimpleEventSourcing.ReadModel;
using SQLite;

namespace Shop.ReadModel.Shared
{
    public abstract class BaseReadModel : IReadModel<int?>
    {
        [PrimaryKey]
        [AutoIncrement]
        public int? Id { get; set; }

        [Ignore]
        object IReadModelBase.Id { get => Id; set => Id = (int?)value; }
    }
}
