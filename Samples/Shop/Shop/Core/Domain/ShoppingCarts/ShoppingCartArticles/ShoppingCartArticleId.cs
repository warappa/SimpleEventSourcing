using Shop.Core.Domain.Shared;
using System.Collections.Generic;

namespace Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles
{
    public class ShoppingCartArticleId : BaseChildId<ShoppingCartArticleId, ShoppingCartId>
    {
        public ShoppingCartArticleId()
        {
        }

        public ShoppingCartArticleId(string value, ShoppingCartId shoppingCartId)
            : base(value, shoppingCartId)
        {
        }

        public static implicit operator string(ShoppingCartArticleId value)
        {
            return ConvertToStringId(value);
        }
        public static implicit operator ShoppingCartArticleId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Empty;
            }

            return ConvertToId(value);
        }

        public static bool operator ==(ShoppingCartArticleId left, ShoppingCartArticleId right)
        {
            return EqualityComparer<ShoppingCartArticleId>.Default.Equals(left, right);
        }

        public static bool operator !=(ShoppingCartArticleId left, ShoppingCartArticleId right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is string s)
            {
                return s == ConvertToStringId(this);
            }

            var other = obj as ShoppingCartArticleId;

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return other.AggregateRootId?.Equals(AggregateRootId) == true &&
                other.Value == Value;
        }

        public override int GetHashCode()
        {
            var hashCode = 1389681877;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<ShoppingCartId>.Default.GetHashCode(AggregateRootId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
