using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventSourcing;
using SimpleEventSourcing.System.Text.Json.WriteModel;
using SimpleEventSourcing.WriteModel;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static ISimpleEventSourcingBuilder AddSystemTextJson(this ISimpleEventSourcingBuilder builder)
        {
            var binder = new VersionedBinder();
            builder.Services.Replace(new ServiceDescriptor(typeof(ISerializer), new SystemTextJsonSerializer(binder)));

            return builder;
        }
    }
}
