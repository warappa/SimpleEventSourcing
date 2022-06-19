using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventSourcing
{
    public class SimpleEventSourcingBuilder : ISimpleEventSourcingBuilder
    {
        public SimpleEventSourcingBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; private set; }
    }
}
