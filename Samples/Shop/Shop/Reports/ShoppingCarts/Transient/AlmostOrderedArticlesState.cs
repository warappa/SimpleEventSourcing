using Shop.Core.Domain.ShoppingCarts;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shop.Core.Reports.ShoppingCarts.Transient
{
    /// <summary>
    /// in-memory projection
    /// </summary>
    [Versioned("AlmostOrderedArticlesState", 0)]
    public class AlmostOrderedArticlesState : SynchronousEventSourcedState<AlmostOrderedArticlesState>
    {
        public List<ShoppingCartArticleRemovedInfo> AlmostOrderedShoppingCartArticles { get; private set; } = new List<ShoppingCartArticleRemovedInfo>();

        private Dictionary<ShoppingCartId, string> ShoppingCartCustomerNames { get; set; } = new Dictionary<ShoppingCartId, string>();
        private Dictionary<ShoppingCartId, string> ShoppingCartCustomersIds { get; set; } = new Dictionary<ShoppingCartId, string>();
        private List<ShoppingCartArticleRemovedInfo> RemovedShoppingCartArticles { get; set; } = new List<ShoppingCartArticleRemovedInfo>();

        public AlmostOrderedArticlesState Apply(ShoppingCartCreated @event)
        {
            ShoppingCartCustomerNames.Add(@event.Id, @event.CustomerName);
            ShoppingCartCustomersIds.Add(@event.Id, @event.CustomerId);

            return this;
        }

        public AlmostOrderedArticlesState Apply(ShoppingCartArticleRemoved @event)
        {
            RemovedShoppingCartArticles.Add(new ShoppingCartArticleRemovedInfo
            {
                Articlenumber = @event.Articlenumber,
                CustomerId = ShoppingCartCustomersIds[(string)@event.AggregateRootId],
                CustomerName = ShoppingCartCustomerNames[(string)@event.AggregateRootId],
                ShoppingCartId = (string)@event.AggregateRootId,
                RemovedAt = @event.DateTime
            });

            return this;
        }

        public AlmostOrderedArticlesState Apply(ShoppingCartOrdered shoppingCartOrdered)
        {
            var orderedAt = shoppingCartOrdered.DateTime;
            var fiveMinutesUntilPlacingOrder = orderedAt.AddMinutes(-5);

            bool notRelevant(ShoppingCartArticleRemovedInfo removed) =>
                removed.ShoppingCartId == shoppingCartOrdered.Id &&
                removed.RemovedAt < fiveMinutesUntilPlacingOrder;

            RemovedShoppingCartArticles.RemoveAll(notRelevant);

            var removedInLast5Minutes = RemovedShoppingCartArticles
                .Where(x => x.ShoppingCartId == shoppingCartOrdered.Id)
                .ToList();

            removedInLast5Minutes = removedInLast5Minutes
                .Select(x =>
                {
                    x.Timespan = orderedAt - x.RemovedAt;
                    return x;
                })
                .ToList();

            RemovedShoppingCartArticles.RemoveAll(x => x.ShoppingCartId == shoppingCartOrdered.Id);

            AlmostOrderedShoppingCartArticles.AddRange(removedInLast5Minutes);

            return this;
        }

        public class ShoppingCartArticleRemovedInfo
        {
            public string Articlenumber { get; set; }
            public string CustomerName { get; set; }
            public string ShoppingCartId { get; set; }
            public DateTime RemovedAt { get; set; }
            public TimeSpan Timespan { get; set; }
            public string CustomerId { get; set; }
        }
    }
}
