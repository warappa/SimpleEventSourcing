﻿using Shop.Core.Domain.Articles;
using Shop.Core.Domain.Customers;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Core.Domain.ShoppingCarts
{
    public class ShoppingCart : AggregateRoot<ShoppingCartState, ShoppingCartId>
    {
        public ShoppingCart() : base(Enumerable.Empty<IEvent>()) { }

        public ShoppingCart(ShoppingCartId id, CustomerId customerId, string customerName, DateTime? dateTime = null)
            : base(new ShoppingCartCreated(id, customerId, customerName, dateTime ?? DateTime.UtcNow))
        {

        }

        public async Task<ShoppingCartArticle> PlaceArticleAsync(ShoppingCartArticleId shoppingCartArticleId, ArticleId articleId, int quantity, IEventRepository repository)
        {
            ShoppingCartBusinessRules.CanPlaceArticle(shoppingCartArticleId, articleId, repository).Check(this);

            var article = await repository.GetAsync<Article>(articleId);

            var shoppingCartArticle = new ShoppingCartArticle(
                this,
                shoppingCartArticleId,
                article.Id,
                article.State.Articlenumber,
                article.State.Description,
                article.State.Price, quantity,
                article.State.Price * quantity,
                DateTime.UtcNow);

            // or: RaiseEvent(new ShoppingCartArticlePlaced(aggregateRoot.Id, shoppingCartArticleId, articleId, articlenumber, description, price, quantity, total, dateTime ?? DateTime.UtcNow));
            // var shoppingCartArticle = GetChildEntity<ShoppingCartArticle>(shoppingCartArticleId);

            return shoppingCartArticle;
        }

        public void SetShippingAddress(ShippingAddress shippingAddress)
        {
            ShoppingCartBusinessRules.CanSetShippingAddress().Check(this);

            RaiseEvent(new ShoppingCartShippingAddressSet(Id, shippingAddress, DateTime.UtcNow));
        }

        public void Order(IEventRepository repository)
        {
            ShoppingCartBusinessRules.CanOrder(repository).Check(this);

            RaiseEvent(new ShoppingCartOrdered(Id, DateTime.UtcNow));
        }

        public void Cancel()
        {
            ShoppingCartBusinessRules.CanCancel().Check(this);

            RaiseEvent(new ShoppingCartCancelled(Id, DateTime.UtcNow));
        }
    }
}
