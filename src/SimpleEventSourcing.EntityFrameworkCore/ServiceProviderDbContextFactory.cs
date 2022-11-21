using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleEventSourcing.EntityFrameworkCore
{
    public class ServiceProviderDbContextFactory : IDbContextFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceProviderDbContextFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public TDbContext CreateDbContext<TDbContext>()
            where TDbContext : DbContext
        {
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), serviceProvider.GetRequiredService<DbContextOptions<TDbContext>>());
        }
    }
}
