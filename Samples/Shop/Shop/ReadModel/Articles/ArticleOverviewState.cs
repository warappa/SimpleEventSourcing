using Shop.Core.Domain.Articles;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.Articles
{
    [ControlsReadModels(new[] { typeof(ArticleOverviewViewModel) })]
    public class ArticleOverviewState : ReadRepositoryState<ArticleOverviewState>
    {
        public ArticleOverviewState() { throw new NotSupportedException(); }

        public ArticleOverviewState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(ArticleCreated @event)
        {
            var article = new ArticleOverviewViewModel();
            article.ArticleId = @event.Id;
            article.Articlenumber = @event.Articlenumber;

            return InsertAsync(article);
        }

        public Task Apply(ArticleArticlenumberChanged @event)
        {
            return UpdateByStreamnameAsync<ArticleOverviewViewModel>(@event.Id,
                article =>
                {
                    article.Articlenumber = @event.Articlenumber;
                });
        }
    }
}
