using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SimpleEventSourcing.EntityFrameworkCore.WriteModel;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    public class WriteModelDbContext : DbContext
    {
        private readonly string connectionName;

        public WriteModelDbContext(string connectionName = null)
            : base()
        {
            this.connectionName = connectionName ?? "efWrite";

            // this.Database.Log = msg => Debug.WriteLine(msg);
        }

        public DbSet<RawStreamEntry> Commits { get; set; }

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
