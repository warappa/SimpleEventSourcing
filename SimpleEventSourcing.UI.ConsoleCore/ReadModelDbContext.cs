using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SimpleEventSourcing.EntityFrameworkCore.ReadModel;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    public class ReadModelDbContext : DbContext
    {
        private readonly string connectionName;

        public ReadModelDbContext(string connectionName = null)
            : base()
        {
            this.connectionName = connectionName ?? "efRead";
            // this.Database.Log = msg => Debug.WriteLine(msg);
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public DbSet<PersistentEntity> PersistentEntities { get; set; }
        public DbSet<CheckpointInfo> CheckpointInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var cb = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(cb.GetConnectionString(connectionName));

            base.OnConfiguring(optionsBuilder);
        }
    }
}
