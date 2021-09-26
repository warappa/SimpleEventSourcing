using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventSourcing
{
    public interface ISimpleEventSourcingBuilder
    {
        IServiceCollection Services { get; }
    }
}
