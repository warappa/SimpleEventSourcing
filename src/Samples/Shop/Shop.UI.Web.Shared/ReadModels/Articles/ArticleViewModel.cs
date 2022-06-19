using Shop.ReadModel.Shared;
using SQLite;

namespace Shop.ReadModel.Articles
{
    [Table(nameof(ArticleViewModel))]
    public class ArticleViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ArticleId { get { return Streamname; } set { Streamname = value; } }
        public string Articlenumber { get; set; }
        public string Description { get; set; }
        public string PriceIsoCode { get; set; }
        public decimal PriceValue { get; set; }
        public bool Active { get; set; }
    }
}
