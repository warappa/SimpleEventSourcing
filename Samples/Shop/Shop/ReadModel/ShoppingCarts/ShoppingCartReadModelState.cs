﻿using Shop.Core.Domain.ShoppingCarts;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.ShoppingCarts
{
    [ControlsReadModels(new[] { typeof(ShoppingCartViewModel), typeof(ShoppingCartArticleViewModel) })]
    public class ShoppingCartReadModelState : ReadRepositoryState<ShoppingCartReadModelState>
    {
        public ShoppingCartReadModelState() { throw new NotSupportedException(); }

        public ShoppingCartReadModelState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(ShoppingCartCreated @event)
        {
            var shoppingCart = new ShoppingCartViewModel();
            shoppingCart.ShoppingCartId = @event.Id;
            shoppingCart.CustomersId = @event.CustomerId;
            shoppingCart.CustomerName = @event.CustomerName;
            shoppingCart.Status = (int)ShoppingCartStatus.Open;
            shoppingCart.Active = true;
            shoppingCart.CreatedAt = @event.DateTime;

            return InsertAsync(shoppingCart);
        }

        public Task Apply(ShoppingCartOrdered @event)
        {
            return UpdateByStreamnameAsync<ShoppingCartViewModel>(@event.Id,
                shoppingCart =>
                {
                    shoppingCart.Status = (int)ShoppingCartStatus.Ordered;
                });
        }

        public Task Apply(ShoppingCartCancelled @event)
        {
            return UpdateByStreamnameAsync<ShoppingCartViewModel>(@event.Id,
                shoppingCart =>
                {
                    shoppingCart.Status = (int)ShoppingCartStatus.Cancelled;
                });
        }

        public Task Apply(ShoppingCartArticlePlaced @event)
        {
            return UpdateByStreamnameAsync<ShoppingCartViewModel>((string)@event.AggregateRootId,
                shoppingCart =>
                {
                    var shoppingCartArticle = new ShoppingCartArticleViewModel
                    {
                        Streamname = (string)@event.AggregateRootId,
                        ShoppingCartId = (string)@event.AggregateRootId,
                        ShoppingCartArticleId = (string)@event.Id,
                        Id = shoppingCart.Id,
                        ArticleId = @event.ArticleId,
                        Articlenumber = @event.Articlenumber,
                        Description = @event.Description,
                        CreatedAt = @event.DateTime,
                        PriceValue = @event.Price.Value,
                        PriceIsoCode = @event.Price.IsoCode,
                        TotalValue = @event.Total.Value,
                        TotalIsoCode = @event.Total.IsoCode,
                        Quantity = @event.Quantity
                    };

                    readRepository.InsertAsync(shoppingCartArticle).Wait();
                });
        }

        public Task Apply(ShoppingCartArticleRemoved @event)
        {
            return QueryAndUpdateAsync<ShoppingCartArticleViewModel>(x => x.ShoppingCartArticleId == @event.Id,
                shoppingCartArticle =>
                {
                    shoppingCartArticle.RemovedAt = @event.DateTime;
                });
        }
    }
}
