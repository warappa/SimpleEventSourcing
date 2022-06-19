using Shop.Core.Domain.Customers;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.Customers
{
    [ControlsReadModels(new[] { typeof(CustomerOverviewViewModel) })]
    public class CustomerOverviewState : ReadRepositoryProjector<CustomerOverviewState>
    {
        public CustomerOverviewState() { throw new NotSupportedException(); }

        public CustomerOverviewState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(CustomerCreated @event)
        {
            var customer = new CustomerOverviewViewModel
            {
                CustomerId = @event.Id,
                Name = @event.Name
            };

            return InsertAsync(customer);
        }

        public Task Apply(CustomerRenamed @event)
        {
            return UpdateByStreamnameAsync<CustomerOverviewViewModel>(@event.Id,
                customer =>
                {
                    customer.Name = @event.NewName;
                });
        }
    }
}
