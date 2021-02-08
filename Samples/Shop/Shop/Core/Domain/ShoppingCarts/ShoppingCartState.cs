using Shop.Core.Domain.Customers;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Shop.Core.Domain.ShoppingCarts
{
    [Versioned("ShoppingCartState", 0)]
    public class ShoppingCartState : AggregateRootState<ShoppingCartState, ShoppingCartId>
    {
        public CustomerId CustomerId { get; private set; }
        public DateTime CreatedAm { get; private set; }
        public bool Active { get; private set; }
        public ShoppingCartStatus ShoppingCartStatus { get; private set; }

        public ShoppingCartState()
        {
            ChildStateCreationMap.Add(typeof(ShoppingCartArticlePlaced), evt => new ShoppingCartArticleState((ShoppingCartArticlePlaced)evt));
        }

        public ShoppingCartState(ShoppingCartState state)
            : base(state)
        {
            CustomerId = state.CustomerId;
            CreatedAm = state.CreatedAm;
            Active = state.Active;
            ShoppingCartStatus = state.ShoppingCartStatus;
        }

        public List<ShoppingCartArticleState> ShoppingCartArticleStates => ChildStates.OfType<ShoppingCartArticleState>().ToList();

        public ShoppingCartState Apply(ShoppingCartArticleRemoved @event)
        {
            Debug.WriteLine($"Article removed '{@event.Articlenumber}'");

            return this;
        }

        public ShoppingCartState Apply(ShoppingCartCreated @event)
        {
            return new ShoppingCartState(this)
            {
                StreamName = @event.Id,
                CustomerId = @event.CustomerId,
                CreatedAm = @event.DateTime,
                Active = true,
                ShoppingCartStatus = ShoppingCartStatus.Open
            };
        }

        public void Apply(ShoppingCartOrdered @event)
        {
            ShoppingCartStatus = ShoppingCartStatus.Ordered;
        }

        public void Apply(ShoppingCartCancelled @event)
        {
            ShoppingCartStatus = ShoppingCartStatus.Cancelled;
        }

        public override object ConvertFromStreamName(Type tkey, string streamName)
        {
            return new ShoppingCartId(streamName);
        }

        public override string ConvertToStreamName(Type tkey, object id)
        {
            return ((ShoppingCartId)id).Value;
        }
    }
}
