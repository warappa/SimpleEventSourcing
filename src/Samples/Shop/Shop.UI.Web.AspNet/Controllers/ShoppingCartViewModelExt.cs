using Shop.UI.Web.Shared.ReadModels.ShoppingCarts;
using System.Collections.Generic;

namespace Shop.UI.Web.AspNet.Controllers
{
    public class ShoppingCartViewModelExt : ShoppingCartViewModel
    {
        public List<ShoppingCartArticleViewModel> ShoppingCartArticles { get; set; }
    }
}
