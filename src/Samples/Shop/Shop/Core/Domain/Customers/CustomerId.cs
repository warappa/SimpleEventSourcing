using Shop.Core.Domain.Shared;

namespace Shop.Core.Domain.Customers
{
    public class CustomerId : BaseAggregateId<CustomerId>
    {
        public CustomerId()
        {
        }

        public CustomerId(string value)
            : base(value)
        {
        }

        public static implicit operator string(CustomerId value)
        {
            return value?.Value;
        }
        public static implicit operator CustomerId(string value)
        {
            return string.IsNullOrEmpty(value) ? Empty : new CustomerId(value);
        }

        public override bool Equals(object obj)
        {
            var other = obj as CustomerId;
            if (other is null)
            {
                return false;
            }

            return other.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
