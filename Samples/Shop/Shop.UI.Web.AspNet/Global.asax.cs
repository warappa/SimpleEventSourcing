using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Shop.UI.Web
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            NativeLibraryHack.DoHack();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Program.InitializeAsync().Wait();
        }
    }
}
