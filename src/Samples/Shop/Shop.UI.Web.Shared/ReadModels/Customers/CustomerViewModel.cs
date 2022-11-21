using SQLite;

namespace Shop.UI.Web.Shared.ReadModels.Customers
{
    [Table(nameof(CustomerViewModel))]
    public class CustomerViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string CustomerId { get => Streamname; set => Streamname = value; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}
