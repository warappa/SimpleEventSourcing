using System.Web.Mvc;

namespace Shop.UI.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Shop";

            return View();
        }
    }
}
