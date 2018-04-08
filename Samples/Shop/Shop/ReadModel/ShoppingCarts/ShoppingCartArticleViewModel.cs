using Shop.ReadModel.Shared;
using SQLite;
using System;

namespace Shop.ReadModel.ShoppingCarts
{
    [Table(nameof(ShoppingCartArticleViewModel))]
    public class ShoppingCartArticleViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ShoppingCartId { get { return Streamname; } set { Streamname = value; } }
        public string ShoppingCartArticleId { get; set; }

        public string ArticleId { get; set; }
        public string Articlenumber { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal PriceValue { get; set; }
        public string PriceIsoCode { get; set; }
        public DateTime? RemovedAt { get; set; }
        public string TotalIsoCode { get; set; }
        public decimal TotalValue { get; set; }
        public int Quantity { get; set; }
    }
}
