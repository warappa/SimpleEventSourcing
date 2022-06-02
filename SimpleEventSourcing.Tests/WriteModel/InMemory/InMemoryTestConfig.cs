using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.InMemory;
using SimpleEventSourcing.ReadModel.InMemory.Tests;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.InMemory.Tests
{
    public class InMemoryTestConfig : TestsBaseConfig
    {
        public new ReadModelInMemoryConfig ReadModel => (ReadModelInMemoryConfig)base.ReadModel;
        public new WriteModelInMemoryConfig WriteModel => (WriteModelInMemoryConfig)base.WriteModel;
        public new StorageInMemoryConfig Storage => (StorageInMemoryConfig)base.Storage;

        public InMemoryTestConfig()
        {
            base.ReadModel = new ReadModelInMemoryConfig(this);
            base.WriteModel = new WriteModelInMemoryConfig(this);
            base.Storage = new StorageInMemoryConfig(this);
        }

        public TestEvent GetTestEvent()
        {
            return new TestEvent(RawStreamEntryStreamname, TesteventValue);
        }

        public class StorageInMemoryConfig : StorageConfig
        {
            private InMemoryTestConfig parent;

            public StorageInMemoryConfig(InMemoryTestConfig parent )
            {
                this.parent = parent;
            }
        }

        public class WriteModelInMemoryConfig : WriteModelConfig
        {
            private IPersistenceEngine persistenceEngine;
            private InMemoryTestConfig parent;

            public WriteModelInMemoryConfig(InMemoryTestConfig parent)
            {
                this.parent = parent;
            }

            public override async Task EnsureWriteDatabaseAsync()
            {

            }

            public override async Task ResetAsync()
            {
                persistenceEngine = null;
            }

            public override async Task CleanupWriteDatabaseAsync()
            {
                persistenceEngine = null;
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

            private ISerializer cachedSerializer;
            public ISerializer GetSerializer()
            {
                return cachedSerializer ??= new JsonNetSerializer(GetBinder());
            }

            public ISerializationBinder GetBinder()
            {
                return new VersionedBinder();
            }

            public override IPersistenceEngine GetPersistenceEngine()
            {
                return persistenceEngine ??= new PersistenceEngine(GetSerializer());
            }

            public override bool IsTableInDatabase(Type type)
            {
                throw new NotImplementedException();
            }
            public override IStorageResetter GetStorageResetter()
            {
                throw new NotImplementedException();
            }
        }

        public class ReadModelInMemoryConfig : ReadModelConfig
        {
            private IReadRepository readRepository;
            private InMemoryTestConfig parent;

            public ReadModelInMemoryConfig(InMemoryTestConfig parent)
            {
                this.parent = parent;
            }

            public override async Task CleanupReadDatabaseAsync()
            {
                readRepository = null;
            }

            public override IReadRepository GetReadRepository()
            {
                return readRepository ?? (readRepository = new ReadRepository());
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

            public override bool IsTableInDatabase(Type type)
            {
                return (GetReadRepository() as ReadRepository).IsTableInDatabase(type);
            }

            public override IStorageResetter GetStorageResetter()
            {
                return GetReadRepository() as IStorageResetter;
            }

            public override ICheckpointPersister GetCheckpointPersister()
            {
                return new CheckpointPersister();
            }

            public override Type GetCheckpointInfoType()
            {
                return typeof(object);
            }

            public override async Task EnsureReadDatabaseAsync()
            {

            }

            public override IPollingObserverFactory GetPollingObserverFactory(TimeSpan interval)
            {
                return new PollingObserverFactory(parent.WriteModel.GetPersistenceEngine(), interval);
            }
        }
    }
}
