using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shop.UI.Web.AspNetCore.Blazor.Server
{
    public class ReadModelUpdateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public ReadModelUpdateBackgroundService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();
            var engine = scope.ServiceProvider.GetRequiredService<IPersistenceEngine>();
            await engine.InitializeAsync();

            var projectors = scope.ServiceProvider.GetRequiredService<IEnumerable<IProjectionManager>>();
            foreach (var projector in projectors)
            {
                await projector.StartAsync();
            }

            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
