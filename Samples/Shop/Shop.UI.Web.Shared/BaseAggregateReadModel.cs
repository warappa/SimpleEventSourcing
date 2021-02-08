using SimpleEventSourcing.ReadModel;
using SQLite;

namespace Shop.ReadModel.Shared
{
    public abstract class BaseAggregateReadModel : BaseReadModel, IStreamReadModel
    {
        [Indexed]
        public string Streamname { get; set; }
    }
}
