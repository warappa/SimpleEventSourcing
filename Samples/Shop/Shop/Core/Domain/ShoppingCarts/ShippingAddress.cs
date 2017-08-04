using System;

namespace Shop.Core.Domain.ShoppingCarts
{
    public class ShippingAddress : ValueObject<ShippingAddress>
    {
        public string Street { get; private set; }
        public string Housenumber { get; private set; }
        public string City { get; private set; }

        public ShippingAddress(string street, string housenumber, string city)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentNullException(nameof(street));
            if (string.IsNullOrWhiteSpace(housenumber))
                throw new ArgumentNullException(nameof(housenumber));
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentNullException(nameof(city));

            Street = street;
            Housenumber = housenumber;
            City = city;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ShippingAddress;

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return other.City == City &&
                other.Housenumber == Housenumber &&
                other.Street == Street;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (City?.GetHashCode() * 397 ?? 0) ^
                    (Street?.GetHashCode() * 397 ?? 0) ^
                    (Housenumber?.GetHashCode() * 397 ?? 0);
            }
        }
    }
}
