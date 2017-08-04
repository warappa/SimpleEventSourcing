using Shop.Core.Domain.Shared;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.LegacyEvents.Domain.Customers
{
    [Versioned("CustomerRegistered", 0)]
    public class CustomerRegisteredV0 : BaseEvent
    {
        public string Name { get; private set; }

        public CustomerRegisteredV0(
            string id,
            string name,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Name = name;
        }
    }
}
