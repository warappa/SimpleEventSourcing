using SimpleEventSourcing.Storage;
using SQLite.Net;
using System;

namespace SimpleEventSourcing.SQLite.Storage
{
    public class StorageResetter : IStorageResetter
    {
        private readonly SQLiteConnectionWithLock connection;

        public StorageResetter(SQLiteConnectionWithLock connection)
        {
            this.connection = connection;
        }

        public void Reset(Type[] entityTypes, bool justDrop = false)
        {
            connection.RunInLock((SQLiteConnection c) =>
            {
                foreach (var type in entityTypes)
                {
                    try
                    {
                        c.DeleteAll(type);
                    }
                    catch
                    {
                        // TODO: error handling
                    }

                    if (!justDrop)
                    {
                        c.CreateTable(type);
                    }
                }
            });
        }
    }
}
