using SQLite;

namespace Shop.UI.Web.Shared.ReadModels.ShoppingCarts
{
    [Table(nameof(ShoppingCartOverviewViewModel))]
    public class ShoppingCartOverviewViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ShoppingCartId { get => Streamname; set => Streamname = value; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int Status { get; set; }
    }
}
