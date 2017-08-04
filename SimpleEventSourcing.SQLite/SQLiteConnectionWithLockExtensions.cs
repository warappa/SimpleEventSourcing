using System;

namespace SQLite.Net
{
    public static class SQLiteConnectionWithLockExtensions
    {
        public static void RunInLock(this SQLiteConnectionWithLock connection, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (connection.Lock())
            {
                //var savepoint = this.SaveTransactionPoint();
                try
                {
                    action();

                    //this.Release(savepoint);
                }
                catch (Exception)
                {
                    //this.RollbackTo(savepoint);
                    throw;
                }
            }
        }

        public static void RunInLock(this SQLiteConnectionWithLock connection, Action<SQLiteConnection> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (connection.Lock())
            {
                //var savepoint = this.SaveTransactionPoint();
                try
                {
                    action(connection);

                    //this.Release(savepoint);
                }
                catch (Exception)
                {
                    //this.RollbackTo(savepoint);
                    throw;
                }
            }
        }
    }
}
