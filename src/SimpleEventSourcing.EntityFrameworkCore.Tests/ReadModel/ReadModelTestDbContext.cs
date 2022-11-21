using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using SimpleEventSourcing.EntityFrameworkCore.ReadModel;
using SimpleEventSourcing.EntityFrameworkCore.Tests.Storage;
using SimpleEventSourcing.Tests.ReadModel;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.ReadModel
{
    public class ReadModelTestDbContext : DbContext
    {
        public ReadModelTestDbContext()
            : base(new DbContextOptionsBuilder()
                  .UseSqlServer(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("integrationtest"))
                  .ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning))
                  .Options)
        {
            //Database.SetInitializer<ReadModelTestDbContext>(null);

            //this.Database.Log = msg => Debug.WriteLine(msg);
        }

        public DbSet<CheckpointInfo> CheckpointInfos { get; set; }

        public DbSet<ReadModelTestEntity> ReadModelTestEntities { get; set; }

        public DbSet<TestEntityA> TestEntityAs { get; set; }
        public DbSet<TestEntityB> TestEntityBs { get; set; }

        public DbSet<CatchUpReadModel> CatchUpReadModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CompoundKeyTestEntity>()
                .HasKey(x => new { x.Key1, x.Key2 });

            modelBuilder.Entity<TestEntityA>()
                .HasMany(x => x.SubItems)
                .WithOne()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
