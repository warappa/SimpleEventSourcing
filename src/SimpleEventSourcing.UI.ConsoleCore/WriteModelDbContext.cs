using Microsoft.EntityFrameworkCore;
using SimpleEventSourcing.EntityFrameworkCore.WriteModel;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    public class WriteModelDbContext : DbContext
    {
        public WriteModelDbContext(DbContextOptions<WriteModelDbContext> dbContextOptions)
            : base(dbContextOptions)
        {

        }

        public DbSet<RawStreamEntry> Commits { get; set; }
        public DbSet<RawSnapshot> Snapshots { get; set; }
    }
}
