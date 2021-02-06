using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventSourcing.Bus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBus(this IServiceCollection services)
        {
            services.AddSingleton<IObservableMessageBus, ObservableMessageBus>();

            return services;
        }
    }
}
