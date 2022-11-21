using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Benchmarking.ReadModel;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.EntityFrameworkCore;
using SimpleEventSourcing.Newtonsoft;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.System.Text.Json;

namespace SimpleEventSourcing.Benchmarking.EFCore
{
    internal class SetupEFCore
    {
        public static IServiceProvider BuildEntityFrameworkCore(IConfigurationRoot configuration, bool systemTextJson, bool readModel = false)
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

            var esBuilder = services.AddSimpleEventSourcing<WriteModelDbContext, ReadModelDbContext>();

            if (systemTextJson)
            {
                esBuilder.AddSystemTextJson();
            }
            else
            {
                esBuilder.AddNewtonsoftJson();
            }

            //services.AddCatchupProjector<TestState, ReadModelDbContext>(new TestState());

            if (readModel)
            {
                services.AddCatchupProjector<PersistentState>(
                    sp => new PersistentState(sp.GetRequiredService<IReadRepository>()));
            }

            services.AddBus();

            var serviceProvider = services.BuildServiceProvider();

            using (var writeDbContext = serviceProvider.GetRequiredService<WriteModelDbContext>())
            {
                writeDbContext.Database.EnsureDeleted();
                writeDbContext.Database.EnsureCreated();
            }

            using (var readDbContext = serviceProvider.GetRequiredService<ReadModelDbContext>())
            {
                readDbContext.Database.EnsureDeleted();
                readDbContext.Database.EnsureCreated();
            }

            return serviceProvider;
        }
    }
}
