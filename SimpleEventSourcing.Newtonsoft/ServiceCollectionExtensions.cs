using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Newtonsoft
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNewtonsoftJson(this IServiceCollection services)
        {
            var binder = new VersionedBinder();
            services.AddSingleton<ISerializer>(new JsonNetSerializer(binder));
            return services;
        }
    }
}
