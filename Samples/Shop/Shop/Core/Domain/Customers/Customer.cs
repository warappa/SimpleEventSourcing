using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Linq;

namespace Shop.Core.Domain.Customers
{
    public class Customer : AggregateRoot<CustomerState, CustomerId>
    {
        public Customer() : base(Enumerable.Empty<IEvent>()) { }

        public Customer(CustomerId id, string name)
            : base(new CustomerCreated(id, name, DateTime.UtcNow))
        {
            if (id == CustomerId.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            if (name == string.Empty)
            {
                throw new ArgumentException(nameof(name));
            }
        }

        public void Rename(string name)
        {
            CustomersBusinessRules.CanRename(name).Check(this);

            RaiseEvent(new CustomerRenamed(Id, name, DateTime.UtcNow));
        }

        public void Deactivate(string reason)
        {
            CustomersBusinessRules.CanDeactivate().Check(this);

            RaiseEvent(new CustomerDeactivated(Id, reason, DateTime.UtcNow));
        }
    }
}
