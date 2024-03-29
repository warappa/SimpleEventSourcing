﻿using EntityFramework.DbContextScope;
using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.EntityFramework.ReadModel;
using SimpleEventSourcing.EntityFramework.Storage;
using SimpleEventSourcing.EntityFramework.Tests.ReadModel;
using SimpleEventSourcing.EntityFramework.Tests.Storage;
using SimpleEventSourcing.EntityFramework.Tests.WriteModel;
using SimpleEventSourcing.EntityFramework.WriteModel;
using SimpleEventSourcing.Newtonsoft.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests.ReadModel;
using SimpleEventSourcing.Tests.WriteModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestEvent = SimpleEventSourcing.EntityFramework.Tests.WriteModel.TestEvent;

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
                using var dbContext = new EmptyDbContext("integrationtest");
                dbContext.Database.CreateIfNotExists();

                dbContext.Database.Connection.Close();

                //await GetStorageResetter().ResetAsync(new[] { typeof(RawStreamEntry), typeof(RawSnapshot) }).ConfigureAwait(false);
            }

            public override async Task CleanupWriteDatabaseAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(RawStreamEntry), typeof(RawSnapshot) }, true).ConfigureAwait(false);
            }

            public override async Task ResetAsync()
            {
                await GetPersistenceEngine().InitializeAsync().ConfigureAwait(false);
            }

            public static ISerializationBinder GetBinder()
            {
                return new VersionedBinder();
            }

            private static ISerializer cachedSerializer;
            public static ISerializer GetSerializer()
            {
                return cachedSerializer ??= new JsonNetSerializer(GetBinder());
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
            private readonly EntityFrameworkTestConfig parent;

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
                return new CheckpointPersister<ReadModelTestDbContext, CheckpointInfo>(new DbContextScopeFactory(new DbContextFactory()), parent.WriteModel.GetPersistenceEngine());
            }

            public override async Task CleanupReadDatabaseAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(TestEntityASubEntity), typeof(TestEntityA), typeof(TestEntityASubItem), typeof(TestEntityB), typeof(CatchUpReadModel), typeof(CheckpointInfo) }, true).ConfigureAwait(false);
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

            public override ITestEntityASubItem GetTestEntityASubItem()
            {
                return new TestEntityASubItem()
                {
                    SubItemValue = Guid.NewGuid().ToString()
                };
            }

            public override ITestEntityB GetTestEntityB()
            {
                return new TestEntityB()
                {
                    Value = Guid.NewGuid().ToString()
                };
            }

            public override ICompoundKeyTestEntity GetCompoundKeyTestEntity()
            {
                return new CompoundKeyTestEntity
                {
                    Value = Guid.NewGuid().ToString()
                };
            }

            public override async Task EnsureReadDatabaseAsync()
            {
                using var dbContext = new EmptyDbContext("integrationtest");
                dbContext.Database.CreateIfNotExists();

                dbContext.Database.Connection.Close();
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

            public override IPollingObserverFactory GetPollingObserverFactory(TimeSpan interval)
            {
                return new PollingObserverFactory(parent.WriteModel.GetPersistenceEngine(), interval);
            }
        }

        public class StorageEntityFrameworkConfig : StorageConfig
        {
            private readonly EntityFrameworkTestConfig parent;

            public StorageEntityFrameworkConfig(EntityFrameworkTestConfig parent)
            {
                this.parent = parent;
            }
        }
    }
}
