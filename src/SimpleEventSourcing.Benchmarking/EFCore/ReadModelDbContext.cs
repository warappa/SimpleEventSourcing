using Microsoft.EntityFrameworkCore;
using SimpleEventSourcing.Benchmarking.ReadModel;
using SimpleEventSourcing.EntityFrameworkCore.ReadModel;

namespace SimpleEventSourcing.Benchmarking.EFCore
{
    public class ReadModelDbContext : DbContext
    {
        public ReadModelDbContext(DbContextOptions<ReadModelDbContext> dbContextOptions)
            : base(dbContextOptions)
        {

        }

        public DbSet<PersistentEntity> PersistentEntities { get; set; }
        public DbSet<CheckpointInfo> CheckpointInfos { get; set; }
    }
}
