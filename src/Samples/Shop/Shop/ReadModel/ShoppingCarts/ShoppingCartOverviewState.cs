using Shop.Core.Domain.ShoppingCarts;
using Shop.UI.Web.Shared.ReadModels.ShoppingCarts;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.ShoppingCarts
{
    [ControlsReadModels(new[] { typeof(ShoppingCartOverviewViewModel) })]
    public class ShoppingCartOverviewState : ReadRepositoryProjector<ShoppingCartOverviewState>
    {
        public ShoppingCartOverviewState() { throw new NotSupportedException(); }

        public ShoppingCartOverviewState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(ShoppingCartCreated @event)
        {
            var shoppingCart = new ShoppingCartOverviewViewModel
            {
                ShoppingCartId = @event.Id,
                CustomerId = @event.CustomerId,
                CustomerName = @event.CustomerName,
                Status = (int)ShoppingCartStatus.Open
            };

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
