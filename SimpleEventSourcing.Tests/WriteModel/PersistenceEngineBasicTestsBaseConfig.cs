using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.WriteModel;
using SimpleEventSourcing.WriteModel.InMemory;
using System;

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
            public abstract void EnsureReadDatabase();
            public abstract void CleanupReadDatabase();

            public abstract ITestEntityA GetTestEntityA();
            public abstract ITestEntityB GetTestEntityB();

            public abstract IReadRepository GetReadRepository();

            public abstract bool IsTableInDatabase(Type type);

            public abstract IStorageResetter GetStorageResetter();

            public abstract ICheckpointPersister GetCheckpointPersister();

            public abstract Type GetCheckpointInfoType();
        }

        public abstract class WriteModelConfig
        {
            public abstract IPersistenceEngine GetPersistenceEngine();
            public abstract IRawStreamEntry GenerateRawStreamEntry();

            public abstract void EnsureWriteDatabase();
            public abstract void CleanupWriteDatabase();
            
            public abstract bool IsTableInDatabase(Type type);
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
