using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using SimpleEventSourcing.NHibernate.Context;
using SimpleEventSourcing.NHibernate.ReadModel;
using SimpleEventSourcing.NHibernate.Storage;
using SimpleEventSourcing.NHibernate.WriteModel;
using SimpleEventSourcing.NHibernate.WriteModel.Tests;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.Tests
{
    public class NHibernateTestConfig : TestsBaseConfig
    {
        private const bool IsLoggingEnabled = false;

        public new WriteModelNHConfig WriteModel => (WriteModelNHConfig)base.WriteModel;
        public new ReadModelNHConfig ReadModel => (ReadModelNHConfig)base.ReadModel;
        public new StorageNHConfig Storage => (StorageNHConfig)base.Storage;

        public NHibernateTestConfig()
        {
            if (IsLoggingEnabled)
            {
                Logger.Setup();
            }

            base.ReadModel = new ReadModelNHConfig(this);
            base.WriteModel = new WriteModelNHConfig(this);
            base.Storage = new StorageNHConfig(this);
        }

        public TestEvent GetTestEvent()
        {
            return new TestEvent()
            {
                Value = TesteventValue
            };
        }

        public class WriteModelNHConfig : WriteModelConfig
        {
            private NHibernateTestConfig parent;

            private Dictionary<int, ISessionFactory> sessionFactoryMap = new Dictionary<int, ISessionFactory>();

            public WriteModelNHConfig(NHibernateTestConfig parent)
            {
                this.parent = parent;
            }

            public Configuration GetDbCreationConfiguration()
            {
                var cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = IsLoggingEnabled;
                        db.LogSqlInConsole = IsLoggingEnabled;

                        db.BatchSize = 1000;

                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

                if (IsLoggingEnabled)
                {
                    cfg.SetInterceptor(new SqlStatementInterceptor());
                }

                return cfg;
            }

            public override async Task EnsureWriteDatabaseAsync()
            {
                var cfg = GetDbCreationConfiguration();

                using (var sessionFactory = GetSessionFactory(cfg))
                using (var session = sessionFactory.OpenSession())
                {
                    var conn = session.Connection;

                    using var cmd = conn.CreateCommand();

                    cmd.CommandText = "create database integrationDatabaseNHWriteModel";

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch { }

                    try
                    {
                        cmd.CommandText = @"use integrationDatabaseNHWriteModel;

create table hibernate_unique_key (
         next_hi INT
    );

insert into hibernate_unique_key values ( 1 );";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            public Configuration GetBaseConfiguration()
            {
                var cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = IsLoggingEnabled;
                        db.LogSqlInConsole = IsLoggingEnabled;

                        db.BatchSize = 1000;
                        //db.Batcher<global::NHibernate.AdoNet.HanaBatchingBatcherFactory>();
                        db.Batcher<global::NHibernate.AdoNet.GenericBatchingBatcherFactory>();
                        db.OrderInserts = false;

                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=integrationDatabaseNHWriteModel;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName)
                    .SetProperty(global::NHibernate.Cfg.Environment.GenerateStatistics, IsLoggingEnabled.ToString().ToLower())
                    .SetProperty(global::NHibernate.Cfg.Environment.TrackSessionId, "false");
                ;

                if (IsLoggingEnabled)
                {
                    cfg.SetInterceptor(new SqlStatementInterceptor());
                }

                return cfg;
            }

            public INHibernateResetConfigurationProvider GetNHibernateResetConfigurationProvider()
            {
                return new NHibernateResetConfigurationProvider(GetBaseConfiguration(), new[] { typeof(TestEntityA).Assembly, typeof(RawStreamEntry).Assembly });
            }

            public Configuration GetConfiguration()
            {
                var p = GetNHibernateResetConfigurationProvider();

                var cfg = p.GetConfigurationForTypes(typeof(RawStreamEntry));

                if (IsLoggingEnabled)
                {
                    cfg.SetInterceptor(new SqlStatementInterceptor());
                }
                return cfg;
            }

            public override async Task CleanupWriteDatabaseAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(RawStreamEntry) }, true);
            }

            public ISerializationBinder GetBinder()
            {
                return new VersionedBinder();
            }

            public ISerializer GetSerializer()
            {
                return new JsonNetSerializer(GetBinder());
            }

            public override IRawStreamEntry GenerateRawStreamEntry()
            {
                var binder = GetBinder();
                var serializer = GetSerializer();

                var testEvent = parent.GetTestEvent();

                return new RawStreamEntry
                {
                    Category = parent.RawStreamEntryCategory,
                    CommitId = Guid.NewGuid().ToString(),
                    DateTime = DateTime.UtcNow,
                    Group = parent.RawStreamEntryGroup,
                    Headers = "{}",
                    MessageId = Guid.NewGuid().ToString(),
                    Payload = serializer.Serialize(testEvent),
                    PayloadType = binder.BindToName(testEvent.GetType()),
                    StreamName = parent.RawStreamEntryStreamname
                };
            }

            public override IPersistenceEngine GetPersistenceEngine()
            {
                return new PersistenceEngine(GetSessionFactory(), GetConfiguration(), GetSerializer());
            }

            public override IStorageResetter GetStorageResetter()
            {
                return new StorageResetter(GetNHibernateResetConfigurationProvider());
            }

            public override bool IsTableInDatabase(Type type)
            {
                var name = type.Name;

                using (var session = GetSessionFactory().OpenSession())
                {
                    var meta = new DatabaseMetadata(session.Connection, new global::NHibernate.Dialect.MsSql2012Dialect());

                    return meta.IsTable(name);
                }
            }

            public ISessionFactory GetSessionFactory(Configuration configuration = null)
            {
                configuration ??= GetConfiguration();

                if (sessionFactoryMap.TryGetValue(configuration.GetHashCode(), out var sessionFactory))
                {
                    return sessionFactory;
                }

                sessionFactory = configuration.BuildSessionFactory();
                sessionFactoryMap[configuration.GetHashCode()] = sessionFactory;
                return sessionFactory;
            }


            public override IRawStreamEntryFactory GetRawStreamEntryFactory()
            {
                return new RawStreamEntryFactory();
            }
        }

        public class ReadModelNHConfig : ReadModelConfig
        {
            private NHibernateTestConfig parent;

            private Dictionary<int, ISessionFactory> sessionFactoryMap = new Dictionary<int, ISessionFactory>();

            public ReadModelNHConfig(NHibernateTestConfig parent)
            {
                this.parent = parent;
            }

            public override Type GetCheckpointInfoType()
            {
                return typeof(CheckpointInfo);
            }

            public override ICheckpointPersister GetCheckpointPersister()
            {
                return new CheckpointPersister<CheckpointInfo>(GetSessionFactory());
            }

            public Configuration GetBaseConfiguration()
            {
                var cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = IsLoggingEnabled;
                        db.LogSqlInConsole = IsLoggingEnabled;

                        db.BatchSize = 1000;

                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=integrationDatabaseNHReadModel;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

                if (IsLoggingEnabled)
                {
                    cfg.SetInterceptor(new SqlStatementInterceptor());
                }

                return cfg;
            }

            public Configuration GetConfiguration(Configuration baseConfiguration = null)
            {
                var p = GetNHibernateResetConfigurationProvider(baseConfiguration);

                var cfg = p.GetConfigurationForTypes(typeof(TestEntityA), typeof(TestEntityB), typeof(CheckpointInfo), typeof(CatchUpReadModel));
                if (IsLoggingEnabled)
                {
                    cfg.SetInterceptor(new SqlStatementInterceptor());
                }
                return cfg;
            }

            public INHibernateResetConfigurationProvider GetNHibernateResetConfigurationProvider(Configuration baseConfiguration = null)
            {
                return new NHibernateResetConfigurationProvider(baseConfiguration ?? GetBaseConfiguration(), new[] { typeof(TestEntityA).Assembly, typeof(CheckpointInfo).Assembly });
            }

            public override async Task CleanupReadDatabaseAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(CheckpointInfo), typeof(TestEntityA), typeof(TestEntityB), typeof(CatchUpReadModel) }, true);
            }

            public override async Task EnsureReadDatabaseAsync()
            {
                var cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = IsLoggingEnabled;
                        db.LogSqlInConsole = IsLoggingEnabled;

                        db.BatchSize = 1000;

                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

                if (IsLoggingEnabled)
                {
                    cfg.SetInterceptor(new SqlStatementInterceptor());
                }

                using (var sessionFactory = GetSessionFactory(cfg))
                using (var session = sessionFactory.OpenSession())
                {
                    var conn = session.Connection;
                    using var cmd = conn.CreateCommand();

                    cmd.CommandText = "create database integrationDatabaseNHReadModel";

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch { }
                    try
                    {
                        cmd.CommandText = @"use integrationDatabaseNHReadModel;
create table hibernate_unique_key (
         next_hi INT
    );

insert into hibernate_unique_key values ( 1 );";
                        cmd.ExecuteNonQuery();
                    }
                    catch { }
                }
            }

            public override IReadRepository GetReadRepository()
            {
                return new ReadRepository(GetSessionFactory());
            }

            public override IStorageResetter GetStorageResetter()
            {
                //INHibernateResetConfigurationProvider nHibernateResetConfigurationProvider = null
                return new StorageResetter(GetNHibernateResetConfigurationProvider());
            }

            public override ITestEntityA GetTestEntityA()
            {
                return new TestEntityA()
                {
                    Value = Guid.NewGuid().ToString()
                };
            }

            public override ITestEntityB GetTestEntityB()
            {
                return new TestEntityB()
                {
                    Value = Guid.NewGuid().ToString()
                };
            }

            public ISessionFactory GetSessionFactory(Configuration configuration = null)
            {
                configuration ??= GetConfiguration();

                if (sessionFactoryMap.TryGetValue(configuration.GetHashCode(), out var sessionFactory))
                {
                    return sessionFactory;
                }

                sessionFactory = configuration.BuildSessionFactory();
                sessionFactoryMap[configuration.GetHashCode()] = sessionFactory;
                return sessionFactory;
            }

            public override bool IsTableInDatabase(Type type)
            {
                var name = type.Name;

                using (var session = GetSessionFactory().OpenSession())
                {
                    var meta = new DatabaseMetadata(session.Connection, new global::NHibernate.Dialect.MsSql2012Dialect());
                    //TABLE_NAME e.g. "hibernate_unique_key"
                    return meta.IsTable(name);
                }
            }

            public override IPollingObserverFactory GetPollingObserverFactory(TimeSpan interval)
            {
                return new PollingObserverFactory(parent.WriteModel.GetPersistenceEngine(), interval);
            }
        }

        public class StorageNHConfig : StorageConfig
        {
            private NHibernateTestConfig parent;

            public StorageNHConfig(NHibernateTestConfig parent)
            {
                this.parent = parent;
            }
        }
    }
}
