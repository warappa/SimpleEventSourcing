using SimpleEventSourcing.Newtonsoft.WriteModel;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.SQLite.ReadModel;
using SimpleEventSourcing.SQLite.Storage;
using SimpleEventSourcing.SQLite.Tests.Storage;
using SimpleEventSourcing.SQLite.WriteModel;
using SimpleEventSourcing.Storage;
using SimpleEventSourcing.Tests.ReadModel;
using SimpleEventSourcing.Tests.WriteModel;
using SimpleEventSourcing.WriteModel;
using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.Tests
{
    public class SQLiteTestConfig : TestsBaseConfig
    {
        public new ReadModelSQLiteConfig ReadModel => (ReadModelSQLiteConfig)base.ReadModel;
        public new WriteModelSQLiteConfig WriteModel => (WriteModelSQLiteConfig)base.WriteModel;
        public new StorageSQLiteConfig Storage => (StorageSQLiteConfig)base.Storage;

        public SQLiteTestConfig()
        {
            base.ReadModel = new ReadModelSQLiteConfig(this);
            base.WriteModel = new WriteModelSQLiteConfig(this);
            base.Storage = new StorageSQLiteConfig(this);
        }

        public TestEvent GetTestEvent()
        {
            return new TestEvent()
            {
                Value = TesteventValue
            };
        }

        public class StorageSQLiteConfig : StorageConfig
        {
            private readonly SQLiteTestConfig parent;

            public StorageSQLiteConfig(SQLiteTestConfig parent)
            {
                this.parent = parent;
            }
        }

        public class WriteModelSQLiteConfig : WriteModelConfig
        {
            protected SQLiteConnectionWithLock writeConnection;
            private readonly SQLiteTestConfig parent;

            public WriteModelSQLiteConfig(SQLiteTestConfig parent)
            {
                this.parent = parent;
            }

            public override async Task EnsureWriteDatabaseAsync()
            {

            }
            public override async Task ResetAsync()
            {
                await GetPersistenceEngine().InitializeAsync().ConfigureAwait(false);
            }

            public void CloseConnection()
            {
                writeConnection?.Close();
                writeConnection?.Dispose();
                writeConnection = null;

                GC.Collect();

                Task.Delay(100).Wait();
            }

            public override async Task CleanupWriteDatabaseAsync()
            {
                CloseConnection();

                File.Delete("writeDatabase.db");
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

            public ISerializationBinder GetBinder()
            {
                return new VersionedBinder();
            }

            private ISerializer cachedSerializer;
            public ISerializer GetSerializer()
            {
                return cachedSerializer ??= new JsonNetSerializer(GetBinder());
            }

            public override IPersistenceEngine GetPersistenceEngine()
            {
                return new PersistenceEngine(GetWriteConnectionFactory(), GetSerializer());
            }

            public Func<SQLiteConnectionWithLock> GetWriteConnectionFactory()
            {
                return () =>
                {
                    if (writeConnection != null)
                    {
                        return writeConnection;
                    }

                    var databaseFile = "writeDatabase.db";

                    var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);

                    writeConnection = new SQLiteConnectionWithLock(connectionString);

                    using (writeConnection.Lock())
                    {
                        writeConnection.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA journal_mode = WAL;", Array.Empty<object>()).ExecuteScalar<int>();
                    }

                    return writeConnection;
                };
            }

            public override bool IsTableInDatabase(Type type)
            {
                var name = type.Name;

                var connection = GetWriteConnectionFactory()();
                var command = connection.CreateCommand("");

                command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}';";

                return command.ExecuteScalar<string>() != null;
            }

            public override IStorageResetter GetStorageResetter()
            {
                return new StorageResetter(GetWriteConnectionFactory()());
            }

            public override IRawStreamEntryFactory GetRawStreamEntryFactory()
            {
                return new RawStreamEntryFactory();
            }
        }

        public class ReadModelSQLiteConfig : ReadModelConfig
        {
            protected SQLiteConnectionWithLock readConnection;
            private readonly SQLiteTestConfig parent;

            public ReadModelSQLiteConfig(SQLiteTestConfig parent)
            {
                this.parent = parent;
            }

            public void CloseConnection()
            {
                readConnection?.Close();
                readConnection?.Dispose();
                readConnection = null;

                GC.Collect();

                Task.Delay(100).Wait();
            }

            public override async Task EnsureReadDatabaseAsync()
            {

            }

            public override async Task CleanupReadDatabaseAsync()
            {
                CloseConnection();

                File.Delete("readDatabase.db");
            }

            public Func<SQLiteConnectionWithLock> GetReadConnectionFactory()
            {
                return () =>
                {
                    if (readConnection != null)
                    {
                        return readConnection;
                    }

                    var databaseFile = "readDatabase.db";

                    var connectionString = new SQLiteConnectionString(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);

                    readConnection = new SQLiteConnectionWithLock(connectionString);

                    using (readConnection.Lock())
                    {
                        readConnection.CreateCommand(@"PRAGMA synchronous = NORMAL;
PRAGMA journal_mode = WAL;", Array.Empty<object>()).ExecuteScalar<int>();
                    }

                    return readConnection;
                };
            }

            public override IReadRepository GetReadRepository()
            {
                return new ReadRepository(GetReadConnectionFactory());
            }

            public override ITestEntityA GetTestEntityA()
            {
                return new TestEntityA()
                {
                    Value = Guid.NewGuid().ToString(),
                    SubData = new SubData() { PropA = "propA", PropB = "propB" }
                };
            }

            public override ITestEntityASubItem GetTestEntityASubItem()
            {
                return new TestEntityASubItem()
                {
                    SubItemValue = Guid.NewGuid().ToString()
                };
            }

            public override ICompoundKeyTestEntity GetCompoundKeyTestEntity()
            {
                throw new NotImplementedException();
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
                var name = type.Name;

                var connection = GetReadConnectionFactory()();
                var command = connection.CreateCommand("");

                command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}';";

                return command.ExecuteScalar<string>() != null;
            }

            public override IStorageResetter GetStorageResetter()
            {
                return new StorageResetter(GetReadConnectionFactory()());
            }

            public override ICheckpointPersister GetCheckpointPersister()
            {
                return new CheckpointPersister<CheckpointInfo>(GetReadConnectionFactory()(), parent.WriteModel.GetPersistenceEngine());
            }

            public override Type GetCheckpointInfoType()
            {
                return typeof(CheckpointInfo);
            }

            public SQLiteConnection GetConnection()
            {
                return GetReadConnectionFactory()();
            }

            public override IPollingObserverFactory GetPollingObserverFactory(TimeSpan interval)
            {
                return new PollingObserverFactory(parent.WriteModel.GetPersistenceEngine(), interval);
            }
        }
    }
}
