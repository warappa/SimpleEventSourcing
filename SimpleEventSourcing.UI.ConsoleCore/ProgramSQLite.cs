using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.SQLite;
using SimpleEventSourcing.Newtonsoft;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;
using SQLite;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    internal class ProgramSQLite
    {
        private static SQLiteConnectionWithLock writeConn;
        private static SQLiteConnectionWithLock readConn;

        public static IServiceProvider BuildSQLite(IConfigurationRoot cb)
        {
            var services = new ServiceCollection();

            services.AddSimpleEventSourcing(ConnectionFactory, ReadConnectionFactory);
            services.AddCatchupProjector<TestState>(new TestState());
            services.AddCatchupProjector<PersistentState>(
                sp => new PersistentState(sp.GetRequiredService<IReadRepository>()));
            services.AddNewtonsoftJson();
            services.AddBus();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        private static async Task Main(string[] args)
        {
            await Program.Run(BuildSQLite);
        }

        private static SQLiteConnectionWithLock ConnectionFactory()
        {
            if (writeConn != null)
            {
                return writeConn;
            }

            //SQLite3.Config(ConfigOption.Serialized);

            var databaseFile = "writeDatabase.db";

            var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);

            writeConn = new SQLiteConnectionWithLock(connectionString);

            using (writeConn.Lock())
            {
                writeConn.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA journal_mode = WAL;", Array.Empty<object>()).ExecuteScalar<int>();
            }

            return writeConn;
        }

        private static SQLiteConnectionWithLock ReadConnectionFactory()
        {
            if (readConn != null)
            {
                return readConn;
            }
            //SQLite3.Config(ConfigOption.Serialized);

            var databaseFile = "readDatabase.db";

            var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);

            readConn = new SQLiteConnectionWithLock(connectionString);

            using (readConn.Lock())
            {
                readConn.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA temp_store = MEMORY;
PRAGMA page_size = 4096;
PRAGMA cache_size = 10000;
PRAGMA journal_mode = WAL;", Array.Empty<object>()).ExecuteScalar<int>();
            }

            return readConn;
        }
    }
}
