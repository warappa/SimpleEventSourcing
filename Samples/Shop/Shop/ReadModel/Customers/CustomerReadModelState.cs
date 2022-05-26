using Shop.Core.Domain.Customers;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.Customers
{
    [ControlsReadModels(new[] { typeof(CustomerViewModel) })]
    public class CustomerReadModelState : ReadRepositoryProjector<CustomerReadModelState>
    {
        public CustomerReadModelState() { throw new NotSupportedException(); }

        public CustomerReadModelState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(CustomerCreated @event)
        {
            var customer = new CustomerViewModel
            {
                CustomerId = @event.Id,
                Name = @event.Name,
                Active = true
            };

            return InsertAsync(customer);
        }

        public Task Apply(CustomerRenamed @event)
        {
            return UpdateByStreamnameAsync<CustomerViewModel>(@event.Id,
                customer =>
                {
                    customer.Name = @event.NewName;
                });
        }
    }
}
