using Shop.Core.Domain.Shared;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles
{
    [Versioned("ShoppingCartArticlePlaced", 0)]
    public class ShoppingCartArticlePlaced : BaseChildEvent
    {
        public string Articlenumber { get; private set; }
        public EventMoney Price { get; private set; }
        public string Description { get; private set; }
        public string ArticleId { get; private set; }
        public int Quantity { get; private set; }
        public EventMoney Total { get; private set; }

        public ShoppingCartArticlePlaced(
            string aggregateRootId,
            string id,
            string articleId,
            string articlenumber,
            string description,
            EventMoney price,
            int quantity,
            EventMoney total,
            DateTime dateTime)
            : base(aggregateRootId, id, dateTime)
        {
            ArticleId = articleId;
            Articlenumber = articlenumber;
            Description = description;
            Price = price;
            Quantity = quantity;
            Total = total;
        }
    }

    [Versioned("ShoppingCartArticleRemoved", 0)]
    public class ShoppingCartArticleRemoved : BaseChildEvent
    {
        public string Articlenumber { get; private set; }
        public EventMoney Price { get; private set; }
        public string Description { get; private set; }
        public string ArticleId { get; private set; }
        public int Quantity { get; private set; }
        public EventMoney Total { get; private set; }

        public ShoppingCartArticleRemoved(
            string aggregateRootId,
            string id,
            string articleId,
            string articlenumber,
            string description,
            EventMoney price,
            int quantity,
            EventMoney total,
            DateTime dateTime)
            : base(aggregateRootId, id, dateTime)
        {
            ArticleId = articleId;
            Articlenumber = articlenumber;
            Description = description;
            Price = price;
            Quantity = quantity;
            Total = total;
        }
    }
}
