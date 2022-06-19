using Shop.Core.BusinessRules;
using Shop.Core.Domain.Shared;
using Shop.Core.Specifications;

namespace Shop.Core.Domain.Articles
{
    public static class ArticleBusinessRules
    {
        public static BusinessRule<Article> CanChangeArticlenumber(string newArticlenumber)
        {
            return new BusinessRule<Article>(
                "Cannot change article number!",
                rule =>
                {
                    rule.AddCheck(ArticleSpecifications.IsActive, "Article is not active!");
                    rule.AddCheck(_ => ArticleSpecifications.StringNotEmpty.IsSatisfiedBy(newArticlenumber), "Article number may not be empty!");
                });
        }

        public static BusinessRule<Article> CanAdjustPrice(Money price)
        {
            return new BusinessRule<Article>(
                "Cannot adjust article price!",
                rule =>
                {
                    rule.AddCheck(ArticleSpecifications.IsActive, "Article is not active!");
                    rule.AddCheck(_ => price.Value >= 0, "Price may not be less than zero!");
                });
        }

        public static BusinessRule<Article> CanDeactivate(string reason)
        {
            return new BusinessRule<Article>(
                "Cannot deactivate article!",
                rule =>
                {
                    rule.AddCheck(ArticleSpecifications.IsActive, "Article is already deactivated!");
                });
        }

        public static BusinessRule<Article> CanActivate(string reason)
        {
            return new BusinessRule<Article>(
                "Cannot activate article!",
                rule =>
                {
                    rule.AddCheck(ArticleSpecifications.IsActive.Not(), "Article is already activated!");
                });
        }
    }
}
