using NHibernate;
using NHibernate.Cfg;
using SimpleEventSourcing.Storage;
using NHibernate.Tool.hbm2ddl;
using System.Data.Common;
using SimpleEventSourcing.ReadModel;
using System;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using SimpleEventSourcing.NHibernate.ReadModel;
using SimpleEventSourcing.NHibernate.WriteModel;
using SimpleEventSourcing.NHibernate.Storage;
using SimpleEventSourcing.NHibernate.WriteModel.Tests;
using SimpleEventSourcing.NHibernate.Context;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.Tests
{
    public class NHibernateTestConfig : TestsBaseConfig
    {
        public new WriteModelNHConfig WriteModel => (WriteModelNHConfig)base.WriteModel;
        public new ReadModelNHConfig ReadModel => (ReadModelNHConfig)base.ReadModel;
        public new StorageNHConfig Storage => (StorageNHConfig)base.Storage;

        public NHibernateTestConfig()
        {
            Logger.Setup();
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
            private ISessionFactory sessionFactory;

            private NHibernateTestConfig parent;

            public WriteModelNHConfig(NHibernateTestConfig parent)
            {
                this.parent = parent;
            }

            public Configuration GetDbCreationConfiguration()
            {
                Configuration cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = true;
                        db.LogSqlInConsole = true;

                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

                cfg.SetInterceptor(new SqlStatementInterceptor());

                return cfg;
            }

            public override async Task EnsureWriteDatabaseAsync()
            {
                Configuration cfg = GetDbCreationConfiguration();

                using (var sessionFactory = GetSessionFactory(cfg))
                using (var session = sessionFactory.OpenSession())
                {
                    var conn = session.Connection;

                    var cmd = conn.CreateCommand();

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
                Configuration cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = true;
                        db.LogSqlInConsole = true;

                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=integrationDatabaseNHWriteModel;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

                cfg.SetInterceptor(new SqlStatementInterceptor());

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
                cfg.SetInterceptor(new SqlStatementInterceptor());
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
                    var meta = new DatabaseMetadata((DbConnection)session.Connection, new global::NHibernate.Dialect.MsSql2012Dialect());

                    return meta.IsTable(name);
                }
            }

            public ISessionFactory GetSessionFactory(Configuration config = null)
            {
                if (config != null)
                {
                    return config.BuildSessionFactory();
                }
                return sessionFactory ?? (sessionFactory = GetConfiguration().BuildSessionFactory());
            }

            public override IRawStreamEntryFactory GetRawStreamEntryFactory()
            {
                return new RawStreamEntryFactory();
            }
        }

        public class ReadModelNHConfig : ReadModelConfig
        {
            private NHibernateTestConfig parent;
            private ISessionFactory sessionFactory;

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
                Configuration cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = true;
                        db.LogSqlInConsole = true;
                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=integrationDatabaseNHReadModel;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

                cfg.SetInterceptor(new SqlStatementInterceptor());

                return cfg;
            }

            public Configuration GetConfiguration(Configuration baseConfiguration = null)
            {
                var p = GetNHibernateResetConfigurationProvider(baseConfiguration);

                var cfg = p.GetConfigurationForTypes(typeof(TestEntityA), typeof(TestEntityB), typeof(CheckpointInfo), typeof(CatchUpReadModel));
                cfg.SetInterceptor(new SqlStatementInterceptor());
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
                Configuration cfg = new Configuration()
                    .DataBaseIntegration(db =>
                    {
                        db.LogFormattedSql = true;
                        db.LogSqlInConsole = true;

                        db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";
                        db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                    })
                    .SetProperty(global::NHibernate.Cfg.Environment.CurrentSessionContextClass, typeof(ScopedLogicalCallSessionContext).AssemblyQualifiedName);

                cfg.SetInterceptor(new SqlStatementInterceptor());

                using (var sessionFactory = GetSessionFactory(cfg))
                using (var session = sessionFactory.OpenSession())
                {
                    var conn = session.Connection;
                    var cmd = conn.CreateCommand();

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
                return new ReadRepository(sessionFactory ?? GetSessionFactory());
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
                if (configuration != null)
                {
                    return configuration.BuildSessionFactory();
                }

                return sessionFactory ?? (sessionFactory = GetConfiguration().BuildSessionFactory());
            }

            public override bool IsTableInDatabase(Type type)
            {
                var name = type.Name;

                using (var session = GetSessionFactory().OpenSession())
                {
                    var meta = new DatabaseMetadata((DbConnection)session.Connection, new global::NHibernate.Dialect.MsSql2012Dialect());
                    //TABLE_NAME e.g. "hibernate_unique_key"
                    return meta.IsTable(name);
                }
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
