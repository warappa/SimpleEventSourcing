using SimpleEventSourcing.Storage;
using SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.Storage
{
    public class StorageResetter : IStorageResetter, IWriteModelStorageResetter, IReadModelStorageResetter
    {
        private readonly SQLiteConnectionWithLock connection;

        public StorageResetter(SQLiteConnectionWithLock connection)
        {
            this.connection = connection;
        }

        public async Task ResetAsync(Type[] entityTypes, bool justDrop = false)
        {
            connection.RunInLock((SQLiteConnection c) =>
            {
                foreach (var type in entityTypes.Reverse().ToList())
                {
                    try
                    {
                        c.DeleteAll(c.GetMapping(type));
                    }
                    catch
                    {
                        // TODO: error handling
                    }

                    if (!justDrop)
                    {
                        try
                        {
                            c.CreateTable(type);
                        }
                        catch
                        {
                            // TODO: error handling
                        }
                    }
                }
            });
        }
    }
}
