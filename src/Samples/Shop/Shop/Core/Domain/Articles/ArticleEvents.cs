using Shop.Core.Domain.Shared;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.Domain.Articles
{
    [Versioned("ArticleCreated", 0)]
    public class ArticleCreated : BaseEvent
    {
        public string Articlenumber { get; private set; }
        public EventMoney Price { get; private set; }
        public string Description { get; private set; }

        public ArticleCreated(
            string id,
            string articlenumber,
            string description,
            EventMoney price,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Articlenumber = articlenumber;
            Description = description;
            Price = price;
        }
    }

    [Versioned("ArticleArticlenumberChanged", 0)]
    public class ArticleArticlenumberChanged : BaseEvent
    {
        public string Articlenumber { get; private set; }

        public ArticleArticlenumberChanged(
            string id,
            string articleNumber,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Articlenumber = articleNumber;
        }
    }

    [Versioned("ArticlePriceAdjusted", 0)]
    public class ArticlePriceAdjusted : BaseEvent
    {
        public EventMoney Price { get; private set; }

        public ArticlePriceAdjusted(
            string id,
            EventMoney price,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Price = price;
        }
    }

    [Versioned("ArticleDeactivated", 0)]
    public class ArticleDeactivated : BaseEvent
    {
        public string Reason { get; private set; }

        public ArticleDeactivated(
            string id,
            string reason,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Reason = reason;
        }
    }

    [Versioned("ArticleActivated", 0)]
    public class ArticleActivated : BaseEvent
    {
        public string Reason { get; private set; }

        public ArticleActivated(
            string id,
            string reason,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Reason = reason;
        }
    }
}
