using Shop.ReadModel.ShoppingCarts;
using System.Collections.Generic;

namespace Shop.UI.Web.AspNetCore.Blazor.Server.Controllers
{
    public class ShoppingCartViewModelExt : ShoppingCartViewModel
    {
        public List<ShoppingCartArticleViewModel> ShoppingCartArticles { get; set; }
    }
}
