using Shop.ReadModel.Shared;
using SQLite.Net.Attributes;

namespace Shop.ReadModel.ShoppingCarts
{
    [Table(nameof(ShoppingCartOverviewViewModel))]
    public class ShoppingCartOverviewViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ShoppingCartId { get { return Streamname; } set { Streamname = value; } }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int Status { get; set; }
    }
}
