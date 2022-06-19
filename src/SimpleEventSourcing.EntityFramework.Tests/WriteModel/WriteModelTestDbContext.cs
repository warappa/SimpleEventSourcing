using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.EntityFramework.WriteModel;
using System.Data.Entity;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    public class WriteModelTestDbContext : DbContext, IDbContext
    {
        public WriteModelTestDbContext()
            : base("integrationtest")
        {
            Database.SetInitializer<WriteModelTestDbContext>(null);

            //Database.Log = msg => Debug.WriteLine(msg);
        }

        public DbSet<RawStreamEntry> Commits { get; set; }
        public DbSet<RawSnapshot> Snapshots { get; set; }
    }
}