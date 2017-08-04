﻿using Shop.Core.Domain.ShoppingCarts;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.ShoppingCarts
{
    [ControlsReadModels(new[] { typeof(ShoppingCartOverviewViewModel) })]
    public class ShoppingCartOverviewState : ReadRepositoryState<ShoppingCartOverviewState>
    {
        public ShoppingCartOverviewState() { throw new NotSupportedException(); }

        public ShoppingCartOverviewState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(ShoppingCartCreated @event)
        {
            var shoppingCart = new ShoppingCartOverviewViewModel();
            shoppingCart.ShoppingCartId = @event.Id;
            shoppingCart.CustomerId = @event.CustomerId;
            shoppingCart.CustomerName = @event.CustomerName;
            shoppingCart.Status = (int)ShoppingCartStatus.Open;

            return InsertAsync(shoppingCart);
        }

        public Task Apply(ShoppingCartOrdered @event)
        {
            return UpdateByStreamnameAsync<ShoppingCartOverviewViewModel>(@event.Id,
                shoppingCart =>
                {
                    shoppingCart.Status = (int)ShoppingCartStatus.Ordered;
                });
        }

        public Task Apply(ShoppingCartCancelled @event)
        {
            return UpdateByStreamnameAsync<ShoppingCartOverviewViewModel>(@event.Id,
                shoppingCart =>
                {
                    shoppingCart.Status = (int)ShoppingCartStatus.Cancelled;
                });
        }
    }
}
