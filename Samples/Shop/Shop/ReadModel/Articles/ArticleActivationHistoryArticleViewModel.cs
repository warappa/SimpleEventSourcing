using Shop.ReadModel.Shared;
using SQLite;

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
