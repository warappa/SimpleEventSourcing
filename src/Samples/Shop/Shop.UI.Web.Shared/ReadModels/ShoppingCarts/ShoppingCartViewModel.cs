using SQLite;
using System;

namespace Shop.UI.Web.Shared.ReadModels.ShoppingCarts
{
    [Table(nameof(ShoppingCartViewModel))]
    public class ShoppingCartViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ShoppingCartId { get => Streamname; set => Streamname = value; }
        public string CustomersId { get; set; }
        public string CustomerName { get; set; }
        public int Status { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
