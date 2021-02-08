using Microsoft.Extensions.DependencyInjection;

namespace Shop.UI.Web.AspNetCore.Blazor.Server
{
    public interface IServiceCollectionModule
    {
        void ConfigureServices(IServiceCollection services);
    }
}
