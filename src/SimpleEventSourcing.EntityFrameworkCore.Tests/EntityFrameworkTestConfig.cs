﻿using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using SimpleEventSourcing.EntityFrameworkCore.ReadModel;
using SimpleEventSourcing.EntityFrameworkCore.Storage;
using SimpleEventSourcing.EntityFrameworkCore.Tests.ReadModel;
using SimpleEventSourcing.EntityFrameworkCore.Tests.Storage;
using SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel;
using SimpleEventSourcing.EntityFrameworkCore.WriteModel;
using SimpleEventSourcing.Newtonsoft.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests.ReadModel;
using SimpleEventSourcing.Tests.WriteModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestEvent = SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel.TestEvent;

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
                using var dbContext = new EmptyDbContext("integrationtest");
                dbContext.Database.EnsureCreated();

                // dbContext.Database.Connection.Close();
            }

            public override async Task ResetAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(RawStreamEntry), typeof(RawSnapshot) }).ConfigureAwait(false);
            }

            public override async Task CleanupWriteDatabaseAsync()
            {
                await GetStorageResetter().ResetAsync(new[] { typeof(RawStreamEntry), typeof(RawSnapshot) }, true).ConfigureAwait(false);
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
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var connectionString = configuration.GetConnectionString("integrationtest");
                var optionsBuilder = new DbContextOptionsBuilder<WriteModelTestDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));

                var options = optionsBuilder.Options;
                return new StorageResetter<WriteModelTestDbContext>(parent.GetDbContextScopeFactory(), options);
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

        public class ReadModelEntityFrameworkCoreConfig : ReadModelConfig
        {
            private readonly EntityFrameworkCoreTestConfig parent;

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
                var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("integrationtest");
                return new StorageResetter<ReadModelTestDbContext>(parent.GetDbContextScopeFactory(), new DbContextOptionsBuilder<ReadModelTestDbContext>().UseSqlServer(connectionString).ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning)).Options);
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
                dbContext.Database.EnsureCreated();

                //dbContext.Database.Connection.Close();
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

        public class StorageEntityFrameworkCoreConfig : StorageConfig
        {
            private readonly EntityFrameworkCoreTestConfig parent;

            public StorageEntityFrameworkCoreConfig(EntityFrameworkCoreTestConfig parent)
            {
                this.parent = parent;
            }
        }
    }
}
