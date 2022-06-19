using Microsoft.EntityFrameworkCore;
using SimpleEventSourcing.EntityFrameworkCore.ReadModel;

namespace SimpleEventSourcing.UI.ConsoleCore
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
