using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using SimpleEventSourcing.Benchmarking.ReadModel;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.Newtonsoft;
using SimpleEventSourcing.NHibernate;
using SimpleEventSourcing.NHibernate.Context;
using SimpleEventSourcing.NHibernate.ReadModel;
using SimpleEventSourcing.NHibernate.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.System.Text.Json;

namespace SimpleEventSourcing.Benchmarking.NHibernate
{
    internal class SetupNH
    {
        public static IServiceProvider BuildNHibernate(IConfigurationRoot configuration, bool systemTextJson, bool readModel = false)
        {
            var services = new ServiceCollection();

            //var t = typeof(System.Data.SqlClient.SqlClientFactory);
            var esBuilder = services.AddSimpleEventSourcing(GetBaseConfiguration(configuration),
                new[]
                {
                    typeof(RawStreamEntry),
                    typeof(CheckpointInfo),
                    typeof(PersistentEntity)
                });

            if (systemTextJson)
            {
                esBuilder.AddSystemTextJson();
            }
            else
            {
                esBuilder.AddNewtonsoftJson();
            }

            //services.AddCatchupProjector(new TestState());

            if (readModel)
            {
                services.AddCatchupProjector<PersistentState>(
                    sp => new PersistentState(sp.GetRequiredService<IReadRepository>()));
            }

            services.AddBus();

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var readModelResetter = scope.ServiceProvider.GetRequiredService<IReadModelStorageResetter>();
                readModelResetter.ResetAsync(new[]
                    {
                        typeof(CheckpointInfo),
                        typeof(PersistentEntity)
                    }, true).Wait();

                readModelResetter.ResetAsync(new[]
                    {
                        typeof(CheckpointInfo),
                        typeof(PersistentEntity)
                    }, false).Wait();

                var writeModelResetter = scope.ServiceProvider.GetRequiredService<IWriteModelStorageResetter>();
                writeModelResetter.ResetAsync(new[]
                    {
                        typeof(RawStreamEntry),
                        typeof(RawSnapshot)
                    }, true).Wait();

                writeModelResetter.ResetAsync(new[]
                    {
                        typeof(RawStreamEntry),
                        typeof(RawSnapshot)
                    }, false).Wait();
            }

            return serviceProvider;
        }

        public static Configuration GetBaseConfiguration(IConfigurationRoot configuration)
        {
            var cfg = new Configuration()
                .Cache(x =>
                {
                    //x.Provider<global::NHibernate.Caches.CoreDistributedCache.SqlServer.>();
                    x.UseQueryCache = true;
                    x.DefaultExpiration = 120;
                })
                .DataBaseIntegration(db =>
                {
                    //db.LogFormattedSql = true;
                    //db.LogSqlInConsole = true;
                    db.BatchSize = 100;
                    db.Batcher<global::NHibernate.AdoNet.GenericBatchingBatcherFactory>();

                    db.ConnectionString = configuration.GetConnectionString("nh");
                    db.Driver<global::NHibernate.Driver.MicrosoftDataSqlClientDriver>();
                    db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                })
                .SetProperty(global::NHibernate.Cfg.Environment.UseSecondLevelCache, "true")
                .SetProperty(global::NHibernate.Cfg.Environment.GenerateStatistics, "false")
                .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName)
                .SetProperty("adonet.batch_size", "100");

            return cfg;
        }

        public static Configuration GetBaseConfigurationWithoutDb(IConfigurationRoot configuration)
        {
            var cfg = new Configuration()
                .DataBaseIntegration(db =>
                {
                    db.LogFormattedSql = false;
                    db.LogSqlInConsole = false;

                    db.ConnectionString = configuration.GetConnectionString("nhWithoutDB");
                    db.Driver<global::NHibernate.Driver.MicrosoftDataSqlClientDriver>();
                    db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();

                })
                .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

            return cfg;
        }
    }
}
