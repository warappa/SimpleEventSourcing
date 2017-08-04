using Shop.Core.Domain.Articles;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.Articles
{
    [ControlsReadModels(new[] { typeof(ArticleActivationHistoryViewModel), typeof(ArticleActivationHistoryArticleViewModel) })]
    public class ArticleActivationHistoryReadModelState : ReadRepositoryState<ArticleActivationHistoryReadModelState>
    {
        public ArticleActivationHistoryReadModelState() { throw new NotSupportedException(); }

        public ArticleActivationHistoryReadModelState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(ArticleCreated @event)
        {
            var article = new ArticleActivationHistoryArticleViewModel();
            article.ArticleId = @event.Id;
            article.Articlenumber = @event.Articlenumber;

            return InsertAsync(article);
        }

        public Task Apply(ArticleArticlenumberChanged @event)
        {
            return UpdateByStreamnameAsync<ArticleActivationHistoryArticleViewModel>(@event.Id,
                article =>
                {
                    article.Articlenumber = @event.Articlenumber;
                });
        }

        public async Task Apply(ArticleDeactivated @event)
        {
            var article = await readRepository.GetByStreamnameAsync<ArticleActivationHistoryArticleViewModel>(@event.Id)
                .ConfigureAwait(false);

            var history = new ArticleActivationHistoryViewModel();
            history.ArticleId = @event.Id;
            history.Articlenumber = article.Articlenumber;
            history.Active = false;
            history.Reason = @event.Reason;
            history.Date = @event.DateTime;

            await InsertAsync(history).ConfigureAwait(false);
        }

        public async Task Apply(ArticleActivated @event)
        {
            var article = await readRepository.GetByStreamnameAsync<ArticleActivationHistoryArticleViewModel>(@event.Id)
                .ConfigureAwait(false);

            var history = new ArticleActivationHistoryViewModel();
            history.ArticleId = @event.Id;
            history.Articlenumber = article.Articlenumber;
            history.Active = true;
            history.Reason = @event.Reason;
            history.Date = @event.DateTime;

            await InsertAsync(history).ConfigureAwait(false);
        }
    }
}
