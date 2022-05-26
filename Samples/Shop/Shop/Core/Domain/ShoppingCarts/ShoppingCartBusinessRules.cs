using Shop.Core.BusinessRules;
using Shop.Core.Domain.Articles;
using Shop.Core.Domain.ShoppingCarts.ShoppingCartArticles;
using Shop.Core.Specifications;
using SimpleEventSourcing.WriteModel;
using System.Linq;

namespace Shop.Core.Domain.ShoppingCarts
{
    public static class ShoppingCartBusinessRules
    {
        public static BusinessRule<ShoppingCart> CanRename(string newName)
        {
            return new BusinessRule<ShoppingCart>(
                "Cannot rename shopping cart!",
                rule =>
                {
                    rule.AddCheck(ShoppingCartSpecifications.IsActive, "Shopping cart is deactivated!");
                    rule.AddCheck(_ => string.IsNullOrWhiteSpace(newName) == false, "New name is empty!");
                });
        }

        public static BusinessRule<ShoppingCart> CanDeactivate()
        {
            return new BusinessRule<ShoppingCart>(
                "Cannot deactivate shopping cart!",
                rule =>
                {
                    rule.AddCheck(ShoppingCartSpecifications.IsActive, "Shopping cart already is deactivated!");
                });
        }

        public static BusinessRule<ShoppingCart> CanPlaceArticle(ShoppingCartArticleId shoppingCartArticleId, ArticleId articleId, IEventRepository repository)
        {
            return new BusinessRule<ShoppingCart>(
                "Cannot place article into shopping cart!",
                rule =>
                {
                    rule.AddCheck(ShoppingCartSpecifications.IsActive, "Shopping cart is deactivated!");
                    rule.AddCheck(_ => shoppingCartArticleId != ShoppingCartArticleId.Empty, "Id is empty!");
                    rule.AddCheck(_ =>
                    {
                        var article = repository.GetAsync<Article>(articleId).Result;

                        return article?.State.Active == true;
                    }, $"Article does not exist or is deactivated!!");
                });
        }

        public static BusinessRule<ShoppingCart> CanOrder(IEventRepository repository)
        {
            return new BusinessRule<ShoppingCart>(
                "Cannot order shopping cart!",
                rule =>
                {
                    rule.AddCheck(ShoppingCartSpecifications.IsActive, "Shopping cart is deactivated!");

                    rule.AddCheck(wk =>
                        wk.State.ShoppingCartStatus == ShoppingCartStatus.Open,
                        "Cannot order shopping cart in current status!");

                    rule.AddCheck(wk =>
                        wk.State.ShoppingCartArticleStates.Any(x => x.Active),
                        "Shopping cart is empty!");

                    rule.AddCheck(shoppingCart =>
                    {
                        foreach (var shoppingCartArticleState in shoppingCart.State.ShoppingCartArticleStates.Where(x => x.Active))
                        {
                            var article = repository.GetAsync<Article>(shoppingCartArticleState.ArticleId)
                                .Result;

                            if (article == null ||
                                !ArticleSpecifications.IsActive.IsSatisfiedBy(article))
                            {
                                return false;
                            }
                        }

                        return true;
                    }, $"Article does not exist or is deactivated!");
                });
        }

        public static BusinessRule<ShoppingCart> CanCancel()
        {
            return new BusinessRule<ShoppingCart>(
                "Shopping cart cannot be cancelled!",
                rule =>
                {
                    rule.AddCheck(ShoppingCartSpecifications.IsActive, "Shopping cart is deactivated!");
                    rule.AddCheck(ShoppingCartSpecifications.IsInStatus(ShoppingCartStatus.Cancelled), "Shopping cart is already cancelled!");
                    rule.AddCheck(
                        ShoppingCartSpecifications.IsInStatus(ShoppingCartStatus.Ordered)
                            .Or(ShoppingCartSpecifications.IsInStatus(ShoppingCartStatus.Open)),
                        $"Cannot cancel shopping cart in current status!");
                });
        }

        public static BusinessRule<ShoppingCart> CanSetShippingAddress()
        {
            return new BusinessRule<ShoppingCart>(
                 "Cannot set shipping address!",
                 rule =>
                 {
                     rule.AddCheck(ShoppingCartSpecifications.IsActive, "Shopping cart is deactivated!");
                     rule.AddCheck(ShoppingCartSpecifications.IsInStatus(ShoppingCartStatus.Open), "Shipping address cannot be set anymore!");
                 });
        }
    }
}
