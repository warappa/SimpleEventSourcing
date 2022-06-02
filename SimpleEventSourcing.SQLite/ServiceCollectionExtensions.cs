using Microsoft.Extensions.DependencyInjection;

using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.SQLite.ReadModel;
using SimpleEventSourcing.SQLite.Storage;
using SimpleEventSourcing.SQLite.WriteModel;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
using SQLite;
using System;

namespace SimpleEventSourcing.SQLite
{
    public static class ServiceCollectionExtensions
    {
        public static ISimpleEventSourcingBuilder AddSimpleEventSourcing(this IServiceCollection services,
            Func<SQLiteConnectionWithLock> connectionFactory,
            Func<SQLiteConnectionWithLock> readConnectionFactory)
        {
            services.AddSingleton<IRawStreamEntryFactory, RawStreamEntryFactory>();
            services.AddSingleton<IInstanceProvider, DefaultInstanceProvider>();

            services.AddScoped<IPersistenceEngine>(sp =>
            {
                var serializer = sp.GetRequiredService<ISerializer>();

                return new PersistenceEngine(connectionFactory, serializer);
            });
            services.AddScoped<IWriteModelStorageResetter>(sp =>
            {
                return new StorageResetter(connectionFactory());
            });
            services.AddScoped<IEventRepository, EventRepository>();

            services.AddScoped<ICheckpointPersister>(sp =>
            {
                return new CheckpointPersister<CheckpointInfo>(readConnectionFactory());
            });
            services.AddScoped<IReadModelStorageResetter>(sp =>
            {
                return new StorageResetter(readConnectionFactory());
            });
            services.AddScoped<IReadRepository>(sp =>
            {
                return new ReadRepository(readConnectionFactory);
            });

            services.AddScoped<IPollingObserverFactory>(sp =>
            {
                var engine = sp.GetRequiredService<IPersistenceEngine>();
                return new PollingObserverFactory(engine, TimeSpan.FromMilliseconds(100));
            });
            services.AddScoped<IObserverFactory>(sp => sp.GetRequiredService<IPollingObserverFactory>());

            return new SimpleEventSourcingBuilder(services);
        }

        public static IServiceCollection AddCatchupProjector<TProjector>(
            this IServiceCollection services, TProjector state)
            where TProjector : class, IProjector, new()
        {
            services.AddScoped<IProjectionManager<TProjector>>(
                sp =>
                {
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    var observerFactory = sp.GetRequiredService<IObserverFactory>();

                    return new CatchUpProjectionManager<TProjector>(state, checkpointPersister, engine, storageResetter, observerFactory);

                });
            services.AddScoped<IProjectionManager>(sp => sp.GetRequiredService<IProjectionManager<TProjector>>());

            return services;
        }

        public static IServiceCollection AddCatchupProjector<TState>(
            this IServiceCollection services, Func<IServiceProvider, TState> stateFactory)
            where TState : class, IProjector, new()
        {
            services.AddScoped<IProjectionManager<TState>>(
                sp =>
                {
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    var observerFactory = sp.GetRequiredService<IObserverFactory>();

                    return new CatchUpProjectionManager<TState>(stateFactory(sp), checkpointPersister, engine, storageResetter, observerFactory);

                });
            services.AddScoped<IProjectionManager>(sp => sp.GetRequiredService<IProjectionManager<TState>>());

            return services;
        }

        public static IServiceCollection AddCatchupProjector<TProjector>(
            this IServiceCollection services)
            where TProjector : class, IProjector, new()
        {
            services.AddScoped<TProjector>();
            services.AddScoped<IProjectionManager<TProjector>>(
                sp =>
                {
                    var projector = sp.GetRequiredService<TProjector>();
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    var observerFactory = sp.GetRequiredService<IObserverFactory>();

                    return new CatchUpProjectionManager<TProjector>(projector, checkpointPersister, engine, storageResetter, observerFactory);
                });
            services.AddScoped<IProjectionManager>(sp => sp.GetRequiredService<IProjectionManager<TProjector>>());

            return services;
        }
    }
}
