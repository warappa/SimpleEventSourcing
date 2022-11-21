using System.Collections.Generic;

namespace Shop.UI.Web.Shared.ReadModels.ShoppingCarts
{
    public class ShoppingCartViewModelExt : ShoppingCartViewModel
    {
        public List<ShoppingCartArticleViewModel> ShoppingCartArticles { get; set; }
    }
}
