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
        public static IServiceCollection AddSimpleEventSourcing(this IServiceCollection services, 
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

            services.AddScoped<ICheckpointPersister>(sp=>
            {
                return new CheckpointPersister<CheckpointInfo>(readConnectionFactory());
            });
            services.AddScoped<IReadModelStorageResetter>(sp=>
            {
                return new StorageResetter(readConnectionFactory());
            });
            services.AddScoped<IReadRepository>(sp=>
            {
                return new ReadRepository(readConnectionFactory);
            });

            services.AddScoped<IPoller>(sp =>
            {
                var engine = sp.GetRequiredService<IPersistenceEngine>();
                return new Poller(engine, 100);
            });

            return services;
        }

        public static IServiceCollection AddCatchupProjector<TState>(
            this IServiceCollection services, TState state, int interval = 100)
            where TState : class, IEventSourcedState<TState>, new()
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

        public static IServiceCollection AddCatchupProjector<TState>(
            this IServiceCollection services, Func<IServiceProvider, TState> stateFactory, int interval = 100)
            where TState : class, IEventSourcedState<TState>, new()
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
