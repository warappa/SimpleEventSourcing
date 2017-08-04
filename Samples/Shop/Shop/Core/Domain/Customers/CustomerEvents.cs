using Shop.Core.Domain.Shared;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.Domain.Customers
{
    [Versioned("CustomerCreated", 0)]
    public class CustomerCreated : BaseEvent
    {
        public string Name { get; private set; }

        public CustomerCreated(
            string id,
            string name,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Name = name;
        }
    }

    [Versioned("CustomerRenamed", 0)]
    public class CustomerRenamed : BaseEvent
    {
        public string NewName { get; private set; }

        public CustomerRenamed(
            string id,
            string newName,
            DateTime dateTime)
            : base(id, dateTime)
        {
            NewName = newName;
        }
    }

    [Versioned("CustomerDeactivated", 0)]
    public class CustomerDeactivated : BaseEvent
    {
        public string Reason { get; private set; }

        public CustomerDeactivated(
            string id,
            string reason,
            DateTime dateTime)
            : base(id, dateTime)
        {
            Reason = reason;
        }
    }
}
