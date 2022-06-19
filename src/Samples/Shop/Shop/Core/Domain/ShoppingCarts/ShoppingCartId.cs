using Shop.Core.Domain.Shared;
using System.Diagnostics;

namespace Shop.Core.Domain.ShoppingCarts
{
    [DebuggerDisplay("{Value}")]
    public class ShoppingCartId : BaseAggregateId<ShoppingCartId>
    {
        public ShoppingCartId()
        {
        }

        public ShoppingCartId(string value)
            : base(value)
        {
        }

        public static implicit operator string(ShoppingCartId value)
        {
            return value?.Value;
        }

        public static implicit operator ShoppingCartId(string value)
        {
            return string.IsNullOrEmpty(value) ? Empty : new ShoppingCartId(value);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ShoppingCartId;
            if (other == null)
            {
                return false;
            }

            return other.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
