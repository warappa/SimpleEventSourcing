using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using SimpleEventSourcing.NHibernate.ReadModel;
using SimpleEventSourcing.NHibernate.Storage;
using SimpleEventSourcing.NHibernate.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
using System;
using System.Linq;

namespace SimpleEventSourcing.NHibernate
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleEventSourcing(this IServiceCollection services, Configuration baseConfiguration, Type[] entityTypes)
        {
            services.AddSingleton<ISessionFactory>(sp =>
            {
                var nHibernateResetConfigurationProvider = sp.GetRequiredService<INHibernateResetConfigurationProvider>();

                var configuration = nHibernateResetConfigurationProvider
                    .GetConfigurationForTypes(entityTypes);

                return configuration.BuildSessionFactory();
            });


            services.AddSingleton<INHibernateResetConfigurationProvider>(sp =>
            {
                var searchAssemblies = entityTypes
                    .Select(x => x.Assembly)
                    .Distinct()
                    .ToList();

                return new NHibernateResetConfigurationProvider(baseConfiguration, searchAssemblies);
            });

            services.AddSingleton<IRawStreamEntryFactory, RawStreamEntryFactory>();
            services.AddSingleton<IInstanceProvider, DefaultInstanceProvider>();

            services.AddScoped<IPersistenceEngine>(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                var serializer = sp.GetRequiredService<ISerializer>();

                var nHibernateResetConfigurationProvider = sp.GetRequiredService<INHibernateResetConfigurationProvider>();

                var configuration = nHibernateResetConfigurationProvider
                    .GetConfigurationForTypes(entityTypes);

                return new PersistenceEngine(sessionFactory, configuration, serializer);
            });
            services.AddScoped<IWriteModelStorageResetter, StorageResetter>();
            services.AddScoped<IEventRepository, EventRepository>();

            services.AddScoped<ICheckpointPersister, CheckpointPersister<CheckpointInfo>>();
            services.AddScoped<IReadModelStorageResetter, StorageResetter>();
            services.AddScoped<IReadRepository, ReadRepository>();

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
            services.AddScoped<IProjector>(sp => sp.GetRequiredService<IProjector<TState>>());

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
            services.AddScoped<IProjector>(sp => sp.GetRequiredService<IProjector<TState>>());

            return services;
        }
    }
}
