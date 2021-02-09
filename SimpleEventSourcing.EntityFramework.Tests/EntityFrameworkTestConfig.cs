using EntityFramework.DbContextScope;
using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.EntityFramework.ReadModel;
using SimpleEventSourcing.EntityFramework.Storage;
using SimpleEventSourcing.EntityFramework.WriteModel;
using SimpleEventSourcing.EntityFramework.WriteModel.Tests;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    public class EntityFrameworkTestConfig : TestsBaseConfig
    {
        public new WriteModelEntityFrameworkConfig WriteModel => (WriteModelEntityFrameworkConfig)base.WriteModel;
        public new ReadModelEntityFrameworkConfig ReadModel => (ReadModelEntityFrameworkConfig)base.ReadModel;
        public new StorageEntityFrameworkConfig Storage => (StorageEntityFrameworkConfig)base.Storage;

        public EntityFrameworkTestConfig()
        {
            base.ReadModel = new ReadModelEntityFrameworkConfig(this);
            base.WriteModel = new WriteModelEntityFrameworkConfig(this);
            base.Storage = new StorageEntityFrameworkConfig(this);
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

        public class WriteModelEntityFrameworkConfig : WriteModelConfig
        {
            private readonly EntityFrameworkTestConfig parent;

            public WriteModelEntityFrameworkConfig(EntityFrameworkTestConfig parent)
            {
                this.parent = parent;
            }

            public override async Task EnsureWriteDatabaseAsync()
            {
                using (var dbContext = new EmptyDbContext("integrationtest"))
                {
                    dbContext.Database.CreateIfNotExists();
                    
                    dbContext.Database.Connection.Close();
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
                return new StorageResetter<WriteModelTestDbContext>(parent.GetDbContextScopeFactory());
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

                    var setMethodGeneric = dbContextType.GetTypeInfo().GetRuntimeMethod(nameof(DbContext.Set), Array.Empty<Type>());

                    var setMethod = setMethodGeneric.MakeGenericMethod(type);

                    var set = (IQueryable)setMethod.Invoke(dbContext, Array.Empty<object>());

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

        public class ReadModelEntityFrameworkConfig : ReadModelConfig
        {
            private EntityFrameworkTestConfig parent;

            public ReadModelEntityFrameworkConfig(EntityFrameworkTestConfig parent)
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
                return new StorageResetter<ReadModelTestDbContext>(parent.GetDbContextScopeFactory());
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
                    dbContext.Database.CreateIfNotExists();

                    dbContext.Database.Connection.Close();
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

                    var setMethodGeneric = dbContextType.GetTypeInfo().GetRuntimeMethod(nameof(DbContext.Set), Array.Empty<Type>());

                    var setMethod = setMethodGeneric.MakeGenericMethod(type);

                    var set = (IQueryable)setMethod.Invoke(dbContext, Array.Empty<object>());

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

            public override IPoller GetPoller(TimeSpan interval)
            {
                return new Poller(parent.WriteModel.GetPersistenceEngine(), interval);
            }
        }

        public class StorageEntityFrameworkConfig : StorageConfig
        {
            private EntityFrameworkTestConfig parent;

            public StorageEntityFrameworkConfig(EntityFrameworkTestConfig parent)
            {
                this.parent = parent;
            }
        }
    }
}
