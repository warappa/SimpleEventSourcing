using SQLite;

namespace SimpleEventSourcing.SQLite
{
    public static class SQLiteConnectionExtensions
    {
        public static bool TableExists(this SQLiteConnection connection, string tableName)
        {
            var cmd = connection.CreateCommand("SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;");
            cmd.Bind("@tableName", tableName);
            return cmd.ExecuteQuery<object>() != null;
        }
    }
}
