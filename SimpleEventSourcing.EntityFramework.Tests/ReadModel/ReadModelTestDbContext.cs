using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.EntityFramework.ReadModel;
using SimpleEventSourcing.ReadModel.Tests;
using SimpleEventSourcing.EntityFramework.WriteModel.Tests;
using System.Data.Entity;
using System.Diagnostics;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    public class ReadModelTestDbContext : DbContext, IDbContext
    {
        public ReadModelTestDbContext()
            : base("integrationtest")
        {
            Database.SetInitializer<ReadModelTestDbContext>(null);

            this.Database.Log = msg => Debug.WriteLine(msg);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public DbSet<CheckpointInfo> CheckpointInfos { get; set; }

        public DbSet<ReadModelTestEntity> ReadModelTestEntities { get; set; }

        public DbSet<TestEntityA> TestEntityAs { get; set; }
        public DbSet<TestEntityB> TestEntityBs { get; set; }

        public DbSet<CatchUpReadModel> CatchUpReadModel { get; set; }
    }
}