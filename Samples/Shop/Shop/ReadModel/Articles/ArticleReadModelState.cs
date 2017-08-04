using Shop.Core.Domain.Articles;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.Articles
{
    [ControlsReadModels(new[] { typeof(ArticleViewModel) })]
    public class ArticleReadModelState : ReadRepositoryState<ArticleReadModelState>
    {
        public ArticleReadModelState() { throw new NotSupportedException(); }

        public ArticleReadModelState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(ArticleCreated @event)
        {
            var article = new ArticleViewModel();
            article.ArticleId = @event.Id;
            article.Articlenumber = @event.Articlenumber;
            article.Description = @event.Description;
            article.PriceIsoCode = @event.Price.IsoCode;
            article.PriceValue = @event.Price.Value;
            article.Active = true;

            return InsertAsync(article);
        }

        public Task Apply(ArticleArticlenumberChanged @event)
        {
            return UpdateByStreamnameAsync<ArticleViewModel>(@event.Id,
                article =>
                {
                    article.Articlenumber = @event.Articlenumber;
                });
        }

        public Task Apply(ArticleDeactivated @event)
        {
            return UpdateByStreamnameAsync<ArticleViewModel>(@event.Id,
                article =>
                {
                    article.Active = false;
                });
        }

        public Task Apply(ArticleActivated @event)
        {
            return UpdateByStreamnameAsync<ArticleViewModel>(@event.Id,
                article =>
                {
                    article.Active = true;
                });
        }
    }
}
