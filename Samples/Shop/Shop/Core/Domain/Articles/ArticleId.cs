using Shop.Core.Domain.Shared;

namespace Shop.Core.Domain.Articles
{
    public class ArticleId : BaseAggregateId<ArticleId>
    {
        public ArticleId()
        {
        }

        public ArticleId(string value)
            : base(value)
        {
        }

        public static implicit operator string(ArticleId value)
        {
            return value?.Value;
        }

        public static implicit operator ArticleId(string value)
        {
            return string.IsNullOrEmpty(value) ? Empty : new ArticleId(value);
        }
    }
}
