using Shop.ReadModel.Shared;
using SQLite;
using System;

namespace Shop.ReadModel.Articles
{
    [Table(nameof(ArticleActivationHistoryViewModel))]
    public class ArticleActivationHistoryViewModel : BaseAggregateReadModel
    {
        [Ignore]
        public string ArticleId { get => Streamname; set => Streamname = value; }
        public string Articlenumber { get; set; }
        public string Reason { get; set; }
        public bool Active { get; set; }
        public DateTime Date { get; set; }
    }
}
