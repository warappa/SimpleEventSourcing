using Shop.Core.Domain.Customers;
using Shop.Core.Domain.Shared;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles
{
    [Versioned("ShoppingCartArticleState", 0)]
    public class ShoppingCartArticleState : ChildEntityState<ShoppingCartArticleState, ShoppingCartId, ShoppingCartArticleId>
    {
        public CustomerId CustomerId { get; private set; }
        public DateTime CreatedAm { get; private set; }
        public bool Active { get; private set; }
        public Money Price { get; private set; }
        public Money Total { get; private set; }
        public string ArticleId { get; private set; }
        public string Articlenumber { get; private set; }
        public string Description { get; private set; }
        public int Quantity { get; private set; }

        public ShoppingCartArticleState() { }
        public ShoppingCartArticleState(ShoppingCartArticlePlaced @event)
        {
            Apply(@event);
        }

        public ShoppingCartArticleState(ShoppingCartArticleState state)
            : base(state)
        {
            CustomerId = state.CustomerId;
            CreatedAm = state.CreatedAm;
            Active = state.Active;
            Price = state.Price;
            Total = state.Total;
            ArticleId = state.ArticleId;
            Articlenumber = state.Articlenumber;
            Description = state.Description;
        }

        public ShoppingCartArticleState Apply(ShoppingCartArticlePlaced @event)
        {
            var s = this;// new ShoppingCartArticleState(this);
            s.AggregateRootId = (string)@event.AggregateRootId;
            s.Id = (string)@event.Id;
            s.ArticleId = @event.ArticleId;
            s.Articlenumber = @event.Articlenumber;
            s.Description = @event.Description;
            s.Price = @event.Price;
            s.Total = @event.Total;
            s.Quantity = @event.Quantity;
            s.CreatedAm = @event.DateTime;
            s.Active = true;

            return s;
        }

        public ShoppingCartArticleState Apply(ShoppingCartArticleRemoved @event)
        {
            var s = new ShoppingCartArticleState(this)
            {
                Active = false
            };

            return s;
        }

        public override object ConvertFromStreamName(Type tkey, string streamName)
        {
            var strs = streamName.Split('|');
            return new ShoppingCartArticleId(strs[1], new ShoppingCartId(strs[0]));
        }

        public override string ConvertToStreamName(Type tkey, object id)
        {
            return ((ShoppingCartArticleId)id).Value;
        }
    }
}
