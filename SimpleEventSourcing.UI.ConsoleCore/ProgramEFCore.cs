using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.EntityFrameworkCore;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    internal class ProgramEFCore
    {
        public static IServiceProvider BuildEntityFrameworkCore(IConfigurationRoot configuration)
        {
            var services = new ServiceCollection();

            services.AddDbContext<WriteModelDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("efWrite"));
            }, ServiceLifetime.Transient);
            services.AddDbContext<ReadModelDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("efRead"));
            }, ServiceLifetime.Transient);

            services.AddSimpleEventSourcing<WriteModelDbContext, ReadModelDbContext>()
                //.AddNewtonsoftJson()
                .AddSystemTextJson()
                ;

            services.AddCatchupProjector(new TestState());
            services.AddCatchupProjector(
                sp => new PersistentState(sp.GetRequiredService<IReadRepository>()));
            
            services.AddBus();

            var serviceProvider = services.BuildServiceProvider();

            using (var writeDbContext = serviceProvider.GetRequiredService<WriteModelDbContext>())
            {
                writeDbContext.Database.EnsureCreated();
            }

            using (var readDbContext = serviceProvider.GetRequiredService<ReadModelDbContext>())
            {
                readDbContext.Database.EnsureCreated();
            }

            return serviceProvider;
        }

        private static async Task Main(string[] args)
        {
            await Program.Run(BuildEntityFrameworkCore);
        }
    }
}
