﻿using Shop.Core.Domain.Articles;
using Shop.UI.Web.Shared.ReadModels.Articles;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.Articles
{
    [ControlsReadModels(new[] { typeof(ArticleOverviewViewModel) })]
    public class ArticleOverviewState : ReadRepositoryProjector<ArticleOverviewState>
    {
        public ArticleOverviewState() { throw new NotSupportedException(); }

        public ArticleOverviewState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(ArticleCreated @event)
        {
            var article = new ArticleOverviewViewModel
            {
                ArticleId = @event.Id,
                Articlenumber = @event.Articlenumber
            };

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
