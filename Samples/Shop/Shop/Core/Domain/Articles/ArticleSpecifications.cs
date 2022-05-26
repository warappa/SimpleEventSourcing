using Shop.Core.Domain.Shared;
using Shop.Core.Specifications;

namespace Shop.Core.Domain.Articles
{
    public static class ArticleSpecifications
    {
        public static readonly ISpecification<Article> ArticlenumberNotEmpty = new Specification<Article>(x => !string.IsNullOrEmpty(x.State.Articlenumber));

        public static readonly ISpecification<string> StringNotEmpty = new Specification<string>(x => !string.IsNullOrEmpty(x));

        public static readonly ISpecification<IAggregateId> AggregateIdNotEmpty = new Specification<IAggregateId>(x => !string.IsNullOrEmpty(x.Value));

        public static readonly ISpecification<Article> IsActive = new Specification<Article>(x => x.State.Active);
    }
}
