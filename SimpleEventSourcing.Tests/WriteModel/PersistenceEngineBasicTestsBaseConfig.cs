using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
using SimpleEventSourcing.WriteModel.InMemory;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests
{
    public abstract class TestsBaseConfig
    {
        public readonly string TesteventValue = "testevent";

        public readonly string RawStreamEntryStreamname = "teststream";
        public readonly string RawStreamEntryGroup = "group";
        public readonly string RawStreamEntryCategory = "category";

        public readonly string TestEntityValue = "testevent";

        public virtual ReadModelConfig ReadModel { get; set; }

        public virtual WriteModelConfig WriteModel { get; set; }

        public virtual StorageConfig Storage { get; set; }

        public abstract class ReadModelConfig
        {
            public abstract Task EnsureReadDatabaseAsync();
            public abstract Task CleanupReadDatabaseAsync();

            public abstract ITestEntityA GetTestEntityA();
            public abstract ITestEntityB GetTestEntityB();

            public abstract IReadRepository GetReadRepository();

            public abstract bool IsTableInDatabase(Type type);

            public abstract IStorageResetter GetStorageResetter();

            public abstract ICheckpointPersister GetCheckpointPersister();

            public abstract Type GetCheckpointInfoType();

            public abstract IPollingObserverFactory GetPollingObserverFactory(TimeSpan interval);
        }

        public abstract class WriteModelConfig
        {
            public abstract IPersistenceEngine GetPersistenceEngine();
            public abstract IRawStreamEntry GenerateRawStreamEntry();

            public abstract Task EnsureWriteDatabaseAsync();
            public abstract Task CleanupWriteDatabaseAsync();
            
            public abstract bool IsTableInDatabase(Type type);

            public abstract Task ResetAsync();

            public abstract IStorageResetter GetStorageResetter();

            public IInstanceProvider GetInstanceProvider()
            {
                return new DefaultInstanceProvider();
            }

            public virtual IRawStreamEntryFactory GetRawStreamEntryFactory()
            {
                return new RawStreamEntryFactory();
            }
        }

        public abstract class StorageConfig
        {
        }
    }
}
