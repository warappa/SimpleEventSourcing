using Shop.ReadModel.Shared;
using SQLite;

namespace Shop.ReadModel.Customers
{
    [Table(nameof(CustomerOverviewViewModel))]
    public class CustomerOverviewViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string CustomerId { get => Streamname; set => Streamname = value; }
        public string Name { get; set; }
    }
}
