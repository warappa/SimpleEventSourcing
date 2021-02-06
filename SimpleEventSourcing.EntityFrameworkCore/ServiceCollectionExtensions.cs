using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.EntityFrameworkCore.ReadModel;
using SimpleEventSourcing.EntityFrameworkCore.Storage;
using SimpleEventSourcing.EntityFrameworkCore.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
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
            return serviceProvider.GetRequiredService<TDbContext>();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleEventSourcing<TWriteDbContext, TReadDbContext>(
            this IServiceCollection services)
            where TWriteDbContext : DbContext
            where TReadDbContext : DbContext
        {
            services.AddSimpleEventSourcing<TWriteDbContext, TReadDbContext, ServiceProviderDbContextFactory>();

            return services;
        }

        public static IServiceCollection AddSimpleEventSourcing<TWriteDbContext, TReadDbContext, TDbContextFactory>(
            this IServiceCollection services)
            where TWriteDbContext : DbContext
            where TReadDbContext : DbContext
            where TDbContextFactory : class, IDbContextFactory
        {
            services.AddSingleton<IRawStreamEntryFactory, RawStreamEntryFactory>();
            services.AddSingleton<IInstanceProvider, DefaultInstanceProvider>();
            services.AddSingleton<IDbContextScopeFactory, DbContextScopeFactory>();
            services.AddSingleton<IDbContextFactory, TDbContextFactory>();
            services.AddSingleton<IDbContextFactory, ServiceProviderDbContextFactory>();

            services.AddScoped<IPersistenceEngine, PersistenceEngine<TWriteDbContext>>();
            services.AddScoped<IWriteModelStorageResetter, StorageResetter<TWriteDbContext>>();
            services.AddScoped<IEventRepository, EventRepository>();

            services.AddScoped<ICheckpointPersister, CheckpointPersister<TReadDbContext, CheckpointInfo>>();
            services.AddScoped<IReadModelStorageResetter, StorageResetter<TReadDbContext>>();
            services.AddScoped<IReadRepository, ReadRepository<TReadDbContext>>();

            services.AddScoped<IPoller>(sp =>
            {
                var engine = sp.GetRequiredService<IPersistenceEngine>();
                return new Poller(engine, 100);
            });

            return services;
        }

        public static IServiceCollection AddCatchupProjector<TState, TReadDbContext>(
            this IServiceCollection services, TState state, int interval = 100)
            where TState : class, IEventSourcedState<TState>, new()
            where TReadDbContext : DbContext
        {
            services.AddScoped<IProjector<TState>>(
                sp =>
                {
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();
                    // TODO: which storage?!
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    return new CatchUpProjector<TState>(state, checkpointPersister, engine, storageResetter, interval);

                });

            return services;
        }

        public static IServiceCollection AddCatchupProjector<TState, TReadDbContext>(
            this IServiceCollection services, Func<IServiceProvider, TState> stateFactory, int interval = 100)
            where TState : class, IEventSourcedState<TState>, new()
            where TReadDbContext : DbContext
        {
            services.AddScoped<IProjector<TState>>(
                sp =>
                {
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();

                    // TODO: which storage?!
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    return new CatchUpProjector<TState>(stateFactory(sp), checkpointPersister, engine, storageResetter, interval);

                });

            return services;
        }
    }
}
