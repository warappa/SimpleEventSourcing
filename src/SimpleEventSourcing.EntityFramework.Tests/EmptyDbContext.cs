using System.Data.Entity;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    public class EmptyDbContext : DbContext
    {
        public EmptyDbContext(string connectionName)
            : base(connectionName)
        {
            Database.SetInitializer<EmptyDbContext>(null);
        }
    }
}