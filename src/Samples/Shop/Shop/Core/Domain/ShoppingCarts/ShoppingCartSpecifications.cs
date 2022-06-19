using Shop.Core.Specifications;

namespace Shop.Core.Domain.ShoppingCarts
{
    public static class ShoppingCartSpecifications
    {
        public static readonly ISpecification<ShoppingCart> IsActive = new Specification<ShoppingCart>(x => x.State.Active);

        public static ISpecification<ShoppingCart> IsInStatus(ShoppingCartStatus status)
        {
            return new Specification<ShoppingCart>(x => x.State.ShoppingCartStatus == status);
        }
    }
}
