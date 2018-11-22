using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    public class EmptyDbContext : DbContext
    {
        public EmptyDbContext(string connectionName)
            : base(new DbContextOptionsBuilder().UseSqlServer(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString(connectionName)).ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning)).Options)
        {
            //Database.SetInitializer<EmptyDbContext>(null);
        }
    }
}