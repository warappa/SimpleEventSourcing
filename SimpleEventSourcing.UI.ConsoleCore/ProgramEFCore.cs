using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Bus;
using SimpleEventSourcing.EntityFrameworkCore;
using SimpleEventSourcing.Newtonsoft;
using SimpleEventSourcing.NHibernate.Storage;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleCore
{
    internal class ProgramEFCore
    {
        public static IServiceProvider BuildEntityFrameworkCore(IConfigurationRoot cb)
        {
            var services = new ServiceCollection();

            services.AddDbContext<WriteModelDbContext>(options =>
            {
                options.UseSqlServer(cb.GetConnectionString("efWrite"));
            }, ServiceLifetime.Transient);
            services.AddDbContext<ReadModelDbContext>(options =>
            {
                options.UseSqlServer(cb.GetConnectionString("efRead"));
            }, ServiceLifetime.Transient);

            services.AddSimpleEventSourcing<WriteModelDbContext, ReadModelDbContext>();
            services.AddCatchupProjector<TestState, ReadModelDbContext>(new TestState());
            services.AddCatchupProjector<PersistentState, ReadModelDbContext>(
                sp => new PersistentState(sp.GetRequiredService<IReadRepository>()));
            services.AddNewtonsoftJson();
            services.AddBus();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        private static async Task Main(string[] args)
        {
            await Program.Run(BuildEntityFrameworkCore);
        }
    }
}
