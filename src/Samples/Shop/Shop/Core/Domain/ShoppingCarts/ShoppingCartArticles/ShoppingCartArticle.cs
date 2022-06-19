using Shop.Core.Domain.Articles;
using Shop.Core.Domain.Shared;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Collections.Generic;

namespace Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles
{
    public class ShoppingCartArticle : ChildEntity<ShoppingCartArticleState, ShoppingCartId, ShoppingCartArticleId>
    {
        public ShoppingCartArticle() { }
        public ShoppingCartArticle(ShoppingCart aggregateRoot, IEnumerable<IChildEntityEvent> events) : base(aggregateRoot, events) { }
        public ShoppingCartArticle(ShoppingCart aggregateRoot, ShoppingCartArticleId shoppingCartArticleId, ArticleId articleId, Articlenumber articlenumber, string description, Money price, int quantity, Money total, DateTime? dateTime)
            : base(aggregateRoot, new[] { new ShoppingCartArticlePlaced(aggregateRoot.Id, shoppingCartArticleId, articleId, articlenumber, description, price, quantity, total, dateTime ?? DateTime.UtcNow) })
        {

        }

        public void RemoveFromShoppingCart()
        {
            RaiseEvent(new ShoppingCartArticleRemoved(aggregateRootId, Id, State.ArticleId, State.Articlenumber, State.Description, State.Price, State.Quantity, State.Total, DateTime.UtcNow));
        }
    }
}
