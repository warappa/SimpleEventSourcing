using Microsoft.Extensions.DependencyInjection;

namespace Shop.UI.Web.AspNetCore.Blazor.Server
{
    public static class ServiceCollectionModuleExtensions
    {
        public static void AddModules(this IServiceCollection services, params IServiceCollectionModule[] modules)
        {
            foreach (var module in modules)
            {
                module.ConfigureServices(services);
            }
        }
    }
}
