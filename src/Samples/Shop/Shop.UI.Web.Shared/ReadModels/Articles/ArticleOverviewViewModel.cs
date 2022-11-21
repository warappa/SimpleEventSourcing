using SQLite;

namespace Shop.UI.Web.Shared.ReadModels.Articles
{
    [Table(nameof(ArticleOverviewViewModel))]
    public class ArticleOverviewViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ArticleId { get => Streamname; set => Streamname = value; }
        public string Articlenumber { get; set; }
    }
}
