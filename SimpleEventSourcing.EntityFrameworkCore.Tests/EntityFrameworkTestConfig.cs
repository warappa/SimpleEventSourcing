using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using SimpleEventSourcing.EntityFrameworkCore.ReadModel;
using SimpleEventSourcing.EntityFrameworkCore.Storage;
using SimpleEventSourcing.EntityFrameworkCore.WriteModel;
using SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    public class EntityFrameworkCoreTestConfig : TestsBaseConfig
    {
        public new WriteModelEntityFrameworkCoreConfig WriteModel => (WriteModelEntityFrameworkCoreConfig)base.WriteModel;
        public new ReadModelEntityFrameworkCoreConfig ReadModel => (ReadModelEntityFrameworkCoreConfig)base.ReadModel;
        public new StorageEntityFrameworkCoreConfig Storage => (StorageEntityFrameworkCoreConfig)base.Storage;

        public EntityFrameworkCoreTestConfig()
        {
            base.ReadModel = new ReadModelEntityFrameworkCoreConfig(this);
            base.WriteModel = new WriteModelEntityFrameworkCoreConfig(this);
            base.Storage = new StorageEntityFrameworkCoreConfig(this);
        }

        public TestEvent GetTestEvent()
        {
            return new TestEvent()
            {
                Value = TesteventValue
            };
        }

        public IDbContextScopeFactory GetDbContextScopeFactory()
        {
            return new DbContextScopeFactory(GetDbContextFactory());
        }

        public IDbContextFactory GetDbContextFactory()
        {
            return new DbContextFactory();
        }

        public class WriteModelEntityFrameworkCoreConfig : WriteModelConfig
        {
            private readonly EntityFrameworkCoreTestConfig parent;

            public WriteModelEntityFrameworkCoreConfig(EntityFrameworkCoreTestConfig parent)
            {
                this.parent = parent;
            }

            public override async Task EnsureWriteDatabaseAsync()
            {
                var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("integrationtest");
                using (var dbContext = new EmptyDbContext("integrationtest"))
                {
                    dbContext.Database.EnsureCreated();

                    // dbContext.Database.Connection.Close();
                }

                //GetStorageResetter().Reset(new[] { typeof(RawStreamEntry) });
            }

            public override async Task CleanupWriteDatabaseAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(RawStreamEntry) }, true);
            }

            public static ISerializationBinder GetBinder()
            {
                return new VersionedBinder();
            }

            public static ISerializer GetSerializer()
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
                    Headers = null,
                    MessageId = Guid.NewGuid().ToString(),
                    Payload = serializer.Serialize(testEvent),
                    PayloadType = binder.BindToName(testEvent.GetType()),
                    StreamName = parent.RawStreamEntryStreamname
                };
            }

            public override IPersistenceEngine GetPersistenceEngine()
            {
                return new PersistenceEngine<WriteModelTestDbContext>(parent.GetDbContextScopeFactory(), GetSerializer());
            }

            public override IStorageResetter GetStorageResetter()
            {
                return new StorageResetter<WriteModelTestDbContext>(parent.GetDbContextScopeFactory(), new DbContextOptionsBuilder().UseSqlServer(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("integrationtest")).ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning)).Options);
            }


            public static WriteModelTestDbContext GetDbContext()
            {
                return new WriteModelTestDbContext();
            }

            public override bool IsTableInDatabase(Type type)
            {
                try
                {
                    var dbContext = GetDbContext();
                    var dbContextType = dbContext.GetType();

                    var setMethodGeneric = dbContextType.GetTypeInfo().GetRuntimeMethod(nameof(DbContext.Set), new Type[0]);

                    var setMethod = setMethodGeneric.MakeGenericMethod(type);

                    var set = (IQueryable)setMethod.Invoke(dbContext, new object[0]);

                    var setType = set.GetType().GetTypeInfo();

                    var anyMethod = typeof(Queryable).GetRuntimeMethods()
                        .Where(x => x.Name == nameof(Queryable.Any))
                        .First(x => x.GetParameters().Length == 0);

                    var anyTyped = anyMethod.MakeGenericMethod(type);

                    anyTyped.Invoke(null, new object[] { set });

                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }

            public override IRawStreamEntryFactory GetRawStreamEntryFactory()
            {
                return new RawStreamEntryFactory();
            }
        }

        public class ReadModelEntityFrameworkCoreConfig : ReadModelConfig
        {
            private EntityFrameworkCoreTestConfig parent;

            public ReadModelEntityFrameworkCoreConfig(EntityFrameworkCoreTestConfig parent)
            {
                this.parent = parent;
            }

            public override Type GetCheckpointInfoType()
            {
                return typeof(CheckpointInfo);
            }

            public override ICheckpointPersister GetCheckpointPersister()
            {
                return new CheckpointPersister<ReadModelTestDbContext, CheckpointInfo>(new DbContextScopeFactory(new DbContextFactory()));
            }

            public override async Task CleanupReadDatabaseAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(TestEntityA), typeof(TestEntityB), typeof(CatchUpReadModel), typeof(CheckpointInfo) }, true);
            }

            public override IReadRepository GetReadRepository()
            {
                var dbContextScopeFactory = parent.GetDbContextScopeFactory();

                return new ReadRepository<ReadModelTestDbContext>(dbContextScopeFactory);
            }

            public override IStorageResetter GetStorageResetter()
            {
                var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("integrationtest");
                return new StorageResetter<ReadModelTestDbContext>(parent.GetDbContextScopeFactory(), new DbContextOptionsBuilder().UseSqlServer(connectionString).ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning)).Options);
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

            public override async Task EnsureReadDatabaseAsync()
            {
                using (var dbContext = new EmptyDbContext("integrationtest"))
                {
                    dbContext.Database.EnsureCreated();

                    //dbContext.Database.Connection.Close();
                }
            }

            public static ReadModelTestDbContext GetDbContext()
            {
                return new ReadModelTestDbContext();
            }


            public override bool IsTableInDatabase(Type type)
            {
                try
                {
                    var dbContext = GetDbContext();
                    var dbContextType = dbContext.GetType();

                    var setMethodGeneric = dbContextType.GetTypeInfo().GetRuntimeMethod(nameof(DbContext.Set), new Type[0]);

                    var setMethod = setMethodGeneric.MakeGenericMethod(type);

                    var set = (IQueryable)setMethod.Invoke(dbContext, new object[0]);

                    var setType = set.GetType().GetTypeInfo();

                    var anyMethod = typeof(Queryable).GetRuntimeMethods()
                        .Where(x => x.Name == nameof(Queryable.Any))
                        .First(x => x.GetParameters().Length == 1);

                    var anyTyped = anyMethod.MakeGenericMethod(type);

                    anyTyped.Invoke(null, new object[] { set });

                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public class StorageEntityFrameworkCoreConfig : StorageConfig
        {
            private EntityFrameworkCoreTestConfig parent;

            public StorageEntityFrameworkCoreConfig(EntityFrameworkCoreTestConfig parent)
            {
                this.parent = parent;
            }
        }
    }
}
