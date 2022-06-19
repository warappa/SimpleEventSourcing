using Shop.Core.Domain.Shared;
using SimpleEventSourcing.WriteModel;
using System;

namespace Shop.Core.Domain.ShoppingCarts
{
    [Versioned("ShoppingCartCreated", 0)]
    public class ShoppingCartCreated : BaseEvent
    {
        public string CustomerId { get; private set; }
        public string CustomerName { get; private set; }

        public ShoppingCartCreated(
            string id,
            string customerId,
            string customerName,
            DateTime dateTime)
            : base(id, dateTime)
        {
            CustomerId = customerId;
            CustomerName = customerName;
        }
    }

    [Versioned("ShoppingCartOrdered", 0)]
    public class ShoppingCartOrdered : BaseEvent
    {
        public ShoppingCartOrdered(
            string id,
            DateTime dateTime)
            : base(id, dateTime)
        {
        }
    }

    [Versioned("ShoppingCartCancelled", 0)]
    public class ShoppingCartCancelled : BaseEvent
    {
        public ShoppingCartCancelled(
            string id,
            DateTime dateTime)
            : base(id, dateTime)
        {
        }
    }

    [Versioned("ShoppingCartShippingAddressSet", 0)]
    public class ShoppingCartShippingAddressSet : BaseEvent
    {
        public ShippingAddressEventData ShippingAddress { get; private set; }

        public ShoppingCartShippingAddressSet(
            string id,
            ShippingAddressEventData shippingAddress,
            DateTime dateTime)
            : base(id, dateTime)
        {
            ShippingAddress = shippingAddress;
        }

        public class ShippingAddressEventData
        {
            public string Street { get; private set; }
            public string Housenumber { get; private set; }
            public string City { get; private set; }

            public ShippingAddressEventData(string street, string housenumber, string city)
            {
                Street = street;
                Housenumber = housenumber;
                City = city;
            }

            public static implicit operator ShippingAddressEventData(ShippingAddress address)
            {
                return new ShippingAddressEventData(address.Street, address.Housenumber, address.City);
            }
        }
    }
}
