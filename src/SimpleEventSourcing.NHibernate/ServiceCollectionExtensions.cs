﻿using Microsoft.Extensions.DependencyInjection;
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
        public static ISimpleEventSourcingBuilder AddSimpleEventSourcing(this IServiceCollection services, Configuration baseConfiguration, Type[] entityTypes)
        {
            services.AddSingleton(sp =>
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

        public static IServiceCollection AddCatchupProjector<TProjector>(
            this IServiceCollection services, Func<IServiceProvider, TProjector> stateFactory)
            where TProjector : class, IProjector, new()
        {
            services.AddScoped<IProjectionManager<TProjector>>(
                sp =>
                {
                    var checkpointPersister = sp.GetRequiredService<ICheckpointPersister>();
                    var engine = sp.GetRequiredService<IPersistenceEngine>();
                    var storageResetter = sp.GetRequiredService<IReadModelStorageResetter>();
                    var observerFactory = sp.GetRequiredService<IObserverFactory>();

                    return new CatchUpProjectionManager<TProjector>(stateFactory(sp), checkpointPersister, engine, storageResetter, observerFactory);
                });
            services.AddScoped<IProjectionManager>(sp => sp.GetRequiredService<IProjectionManager<TProjector>>());

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
