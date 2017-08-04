using SimpleEventSourcing.ReadModel;
using SQLite.Net.Attributes;

namespace Shop.ReadModel.Shared
{
    public abstract class BaseAggregateReadModel : BaseReadModel, IStreamReadModel
    {
        [Indexed]
        public string Streamname { get; set; }
    }
}
