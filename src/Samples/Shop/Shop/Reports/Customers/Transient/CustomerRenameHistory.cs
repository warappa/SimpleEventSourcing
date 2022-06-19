using Shop.Core.Domain.Customers;
using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System.Collections.Generic;
using System.Linq;

namespace Shop.Reports.Customers.Transient
{
    [Versioned("CustomerRenameHistory", 0)]
    public class CustomerRenameHistory : SynchronousEventSourcedState<CustomerRenameHistory>
    {
        public IEnumerable<string> Names { get { return names.ToList(); } }
        public string RenameHistory => string.Join(" -> ", names);

        protected List<string> names = new List<string>();

        public CustomerRenameHistory() { }
        public CustomerRenameHistory(CustomerRenameHistory state)
        {
            names = state.Names.ToList();
        }

        public CustomerRenameHistory Apply(CustomerCreated @event)
        {
            var s = new CustomerRenameHistory(this);
            s.names = s.names.Concat(new[] { @event.Name }).ToList();

            return s;
        }

        public CustomerRenameHistory Apply(CustomerRenamed @event)
        {
            var s = new CustomerRenameHistory(this);
            s.names = s.names.Concat(new[] { @event.NewName }).ToList();

            return s;
        }
    }
}
