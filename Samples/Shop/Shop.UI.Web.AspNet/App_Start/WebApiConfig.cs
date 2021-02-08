using Newtonsoft.Json.Serialization;
using System.Web.Http;

namespace Shop.UI.Web
{
    public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web-API-Konfiguration und -Dienste

			// Web-API-Routen
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApia",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var formatters = GlobalConfiguration.Configuration.Formatters;

            formatters.Remove(formatters.XmlFormatter);
        }
	}
}
