using Shop.ReadModel.Shared;
using SQLite.Net.Attributes;

namespace Shop.ReadModel.Customers
{
    [Table(nameof(CustomerViewModel))]
    public class CustomerViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string CustomerId { get { return Streamname; } set { Streamname = value; } }
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}
