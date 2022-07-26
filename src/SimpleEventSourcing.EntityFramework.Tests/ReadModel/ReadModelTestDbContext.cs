using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.EntityFramework.ReadModel;
using SimpleEventSourcing.EntityFramework.WriteModel.Tests;
using SimpleEventSourcing.ReadModel.Tests;
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

            Database.Log = msg => Debug.WriteLine(msg);
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CompoundKeyTestEntity>()
                .HasKey(x => new { x.Key1, x.Key2 });

            modelBuilder.Entity<TestEntityA>()
                .HasMany(x => x.SubItems)
                .WithRequired()
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete();
        }
    }
}