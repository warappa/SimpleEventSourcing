using Shop.Core.Specifications;

namespace Shop.Core.Domain.Customers
{
    public static class CustomersSpecifications
    {
        public static readonly ISpecification<Customer> IsActive = new Specification<Customer>(x => x.State.Active);
    }
}
