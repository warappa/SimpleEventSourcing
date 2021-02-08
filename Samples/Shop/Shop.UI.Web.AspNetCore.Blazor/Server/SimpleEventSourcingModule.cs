using Microsoft.Extensions.DependencyInjection;
using Shop.ReadModel.Articles;
using Shop.ReadModel.Customers;
using Shop.ReadModel.ShoppingCarts;
using SimpleEventSourcing.Newtonsoft;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.SQLite;
using SQLite;
using System;
using System.IO;
using System.Reflection;

namespace Shop.UI.Web.AspNetCore.Blazor.Server
{
    /// <summary>
    /// Helper class to encapsulate SimpleEventSourcing specific registration
    /// </summary>
    public class SimpleEventSourcingModule : IServiceCollectionModule
    {
        private SQLiteConnectionWithLock writeConnection;
        private SQLiteConnectionWithLock readConnection;

        public void ConfigureServices(IServiceCollection services)
        {
            writeConnection = SetupWriteConnection();
            readConnection = SetupReadConnection();

            services.AddSimpleEventSourcing(() => writeConnection, () => readConnection);
            services.AddNewtonsoftJson();

            services.AddCatchupProjector(sp => new ArticleOverviewState(sp.GetRequiredService<IReadRepository>()));
            services.AddCatchupProjector(sp => new ArticleActivationHistoryReadModelState(sp.GetRequiredService<IReadRepository>()));
            services.AddCatchupProjector(sp => new ArticleReadModelState(sp.GetRequiredService<IReadRepository>()));

            services.AddCatchupProjector(sp => new CustomerOverviewState(sp.GetRequiredService<IReadRepository>()));
            services.AddCatchupProjector(sp => new CustomerReadModelState(sp.GetRequiredService<IReadRepository>()));

            services.AddCatchupProjector(sp => new ShoppingCartOverviewState(sp.GetRequiredService<IReadRepository>()));
            services.AddCatchupProjector(sp => new ShoppingCartReadModelState(sp.GetRequiredService<IReadRepository>()));
        }

        private SQLiteConnectionWithLock SetupWriteConnection()
        {
            var databaseFile = Path.Combine(GetDataPath(), "writeDatabase.db");
            var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache, true, null);
            var writeConn = new SQLiteConnectionWithLock(connectionString)
            {
                BusyTimeout = TimeSpan.FromSeconds(2)
            };
            ConfigureConnection(writeConn);

            return writeConn;
        }

        private SQLiteConnectionWithLock SetupReadConnection()
        {
            var databaseFile = Path.Combine(GetDataPath(), "readDatabase.db");
            var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache, true, null);
            var readConn = new SQLiteConnectionWithLock(connectionString)
            {
                BusyTimeout = TimeSpan.FromSeconds(2)
            };
            ConfigureConnection(readConn);

            return readConn;
        }

        private static void ConfigureConnection(SQLiteConnection conn)
        {
            var pragmas = new[]
{
"PRAGMA journal_mode = WAL;",
//"PRAGMA locking_mode=EXCLUSIVE;",
"PRAGMA cache_size=20000;",
"PRAGMA page_size=32768;",
//"PRAGMA synchronous=off;"
};

            foreach (var pragma in pragmas)
            {
                conn.ExecuteScalar<string>(pragma);
            }
        }

        private static string GetDataPath()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase
                    .Replace("file:///", "")
                    .Replace("/", "\\");
            path = Path.GetDirectoryName(path);

            if (path.EndsWith("bin", StringComparison.Ordinal))
            {
                path = path.Substring(0, path.Length - 4);
            }

            return path;
        }
    }
}
