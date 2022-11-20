using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using SimpleEventSourcing.EntityFrameworkCore.WriteModel;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    public class WriteModelTestDbContext : DbContext
    {
        public WriteModelTestDbContext()
            : base(new DbContextOptionsBuilder().UseSqlServer(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("integrationtest")).ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning)).Options)
        {
            //Database.SetInitializer<WriteModelTestDbContext>(null);

            //this.Database.Log = msg => Debug.WriteLine(msg);
        }

        public DbSet<RawStreamEntry> Commits { get; set; }
        public DbSet<RawSnapshot> Snapshots { get; set; }
    }
}
