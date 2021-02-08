using Shop.ReadModel.ShoppingCarts;
using System.Collections.Generic;

namespace Shop.Web.UI.Controllers
{
    public class ShoppingCartViewModelExt : ShoppingCartViewModel
    {
        public List<ShoppingCartArticleViewModel> ShoppingCartArticles { get; set; }
    }
}
