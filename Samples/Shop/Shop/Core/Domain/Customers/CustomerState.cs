using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.Domain.Customers
{
    [Versioned("CustomerState", 0)]
    public class CustomerState : AggregateRootState<CustomerState, CustomerId>
    {
        public string Name { get; protected set; }
        public bool Active { get; protected set; }

        public CustomerState() { }
        public CustomerState(CustomerState state)
            : base(state)
        {
            Name = state.Name;
            Active = state.Active;
        }

        public CustomerState Apply(CustomerCreated @event)
        {
            var s = new CustomerState(this)
            {
                StreamName = @event.Id,
                Name = @event.Name,
                Active = true
            };

            return s;
        }

        public CustomerState Apply(CustomerRenamed @event)
        {
            var s = new CustomerState(this)
            {
                Name = @event.NewName
            };

            return s;
        }

        public override object ConvertFromStreamName(Type tkey, string streamName)
        {
            return new CustomerId(streamName);
        }

        public override string ConvertToStreamName(Type tkey, object id)
        {
            return ((CustomerId)id).Value;
        }
    }
}
