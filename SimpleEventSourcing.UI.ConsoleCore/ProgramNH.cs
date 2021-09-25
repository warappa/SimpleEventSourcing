using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.Newtonsoft;
using SimpleEventSourcing.NHibernate;
using SimpleEventSourcing.NHibernate.Context;
using SimpleEventSourcing.NHibernate.ReadModel;
using SimpleEventSourcing.NHibernate.WriteModel;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    internal class ProgramNH
    {
        public static Configuration GetBaseConfiguration()
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

                    db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=nhDatabase;";
                    db.Driver<global::NHibernate.Driver.MicrosoftDataSqlClientDriver>();
                    db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                })
                .SetProperty(global::NHibernate.Cfg.Environment.UseSecondLevelCache, "true")
                .SetProperty(global::NHibernate.Cfg.Environment.GenerateStatistics, "true")
                .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName)
                .SetProperty("adonet.batch_size", "100");

            return cfg;
        }

        public static Configuration GetBaseConfigurationWithoutDb()
        {
            var cfg = new Configuration()
                .DataBaseIntegration(db =>
                {
                    db.LogFormattedSql = false;
                    db.LogSqlInConsole = false;

                    db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";
                    db.Driver<global::NHibernate.Driver.MicrosoftDataSqlClientDriver>();
                    db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();

                })
                .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

            return cfg;
        }

        public static IServiceProvider BuildNHibernate(IConfigurationRoot configuration)
        {
            var services = new ServiceCollection();

            //var t = typeof(System.Data.SqlClient.SqlClientFactory);
            services.AddSimpleEventSourcing(GetBaseConfiguration(),
                new[] 
                {
                    typeof(RawStreamEntry), 
                    typeof(CheckpointInfo), 
                    typeof(PersistentEntity)
                });
            services.AddCatchupProjector(new TestState());
            services.AddAsyncCatchupProjector(
                sp => new PersistentState(sp.GetRequiredService<IReadRepository>()));
            services.AddNewtonsoftJson();
            services.AddBus();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        private static async Task Main(string[] args)
        {
            await Program.Run(BuildNHibernate);
        }
    }
}
