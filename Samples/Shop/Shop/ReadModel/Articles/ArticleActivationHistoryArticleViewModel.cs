using SQLite.Net.Attributes;
using Shop.ReadModel.Shared;

namespace Shop.ReadModel.Articles
{
    [Table(nameof(ArticleActivationHistoryArticleViewModel))]
    public class ArticleActivationHistoryArticleViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ArticleId { get => Streamname; set => Streamname = value; }
        public string Articlenumber { get; set; }
    }
}
