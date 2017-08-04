using Shop.Core.Domain.Shared;

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
    }
}
