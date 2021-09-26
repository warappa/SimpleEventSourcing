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
        public static ISimpleEventSourcingBuilder AddSimpleEventSourcing<TWriteDbContext, TReadDbContext>(
            this IServiceCollection services)
            where TWriteDbContext : DbContext
            where TReadDbContext : DbContext
        {
            services.AddSimpleEventSourcing<TWriteDbContext, TReadDbContext, ServiceProviderDbContextFactory>();

            return new SimpleEventSourcingBuilder(services);
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

            services.AddScoped<IPersistenceEngine, PersistenceEngine<TWriteDbContext>>();
            services.AddScoped<IWriteModelStorageResetter, StorageResetter<TWriteDbContext>>();
            services.AddScoped<IEventRepository, EventRepository>();

            services.AddScoped<ICheckpointPersister, CheckpointPersister<TReadDbContext, CheckpointInfo>>();
            services.AddScoped<IReadModelStorageResetter, StorageResetter<TReadDbContext>>();
            services.AddScoped<IReadRepository, ReadRepository<TReadDbContext>>();

            services.AddScoped<IPollingObserverFactory>(sp =>
            {
                var engine = sp.GetRequiredService<IPersistenceEngine>();
                return new PollingObserverFactory(engine, TimeSpan.FromMilliseconds(100));
            });
            services.AddScoped<IObserverFactory>(sp => sp.GetRequiredService<IPollingObserverFactory>());

            return services;
        }

        public static IServiceCollection AddCatchupProjector<TState, TReadDbContext>(
            this IServiceCollection services, TState state)
            where TState : class, IState, new()
            where TReadDbContext : DbContext
        {
            services.AddScoped<IProjector<TState>>(
                sp =>
                {
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    var observerFactory = sp.GetRequiredService<IObserverFactory>();
                    
                    return new CatchUpProjector<TState>(state, checkpointPersister, engine, storageResetter, observerFactory);

                });
            services.AddScoped<IProjector>(sp => sp.GetRequiredService<IProjector<TState>>());

            return services;
        }

        public static IServiceCollection AddCatchupProjector<TState, TReadDbContext>(
            this IServiceCollection services, Func<IServiceProvider, TState> stateFactory)
            where TState : class, IState, new()
            where TReadDbContext : DbContext
        {
            services.AddScoped<IProjector<TState>>(
                sp =>
                {
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    var observerFactory = sp.GetRequiredService<IObserverFactory>();

                    return new CatchUpProjector<TState>(stateFactory(sp), checkpointPersister, engine, storageResetter, observerFactory);

                });
            services.AddScoped<IProjector>(sp => sp.GetRequiredService<IProjector<TState>>());

            return services;
        }
    }
}
