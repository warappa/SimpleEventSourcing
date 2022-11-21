using SQLite;

namespace Shop.UI.Web.Shared.ReadModels.Customers
{
    [Table(nameof(CustomerOverviewViewModel))]
    public class CustomerOverviewViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string CustomerId { get => Streamname; set => Streamname = value; }
        public string Name { get; set; }
    }
}
