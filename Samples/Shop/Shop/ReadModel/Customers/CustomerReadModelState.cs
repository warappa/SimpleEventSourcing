﻿using Shop.Core.Domain.Customers;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace Shop.ReadModel.Customers
{
    [ControlsReadModels(new[] { typeof(CustomerViewModel) })]
    public class CustomerReadModelState : ReadRepositoryState<CustomerReadModelState>
    {
        public CustomerReadModelState() { throw new NotSupportedException(); }

        public CustomerReadModelState(IReadRepository readRepository)
            : base(readRepository)
        {

        }

        public Task Apply(CustomerCreated @event)
        {
            var customer = new CustomerViewModel();
            customer.CustomerId = @event.Id;
            customer.Name = @event.Name;
            customer.Active = true;

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
