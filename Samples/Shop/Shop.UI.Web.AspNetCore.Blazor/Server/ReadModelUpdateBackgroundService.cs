using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleEventSourcing.ReadModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shop.UI.Web.AspNetCore.Blazor.Server
{
    public class ReadModelUpdateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private List<IDisposable> subscriptions = new List<IDisposable>();

        public ReadModelUpdateBackgroundService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var projectors = scope.ServiceProvider.GetRequiredService<IEnumerable<IProjector>>();
                foreach (var projector in projectors)
                {
                    var subscription = await projector.StartAsync();
                    subscriptions.Add(subscription);
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

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var disposable in subscriptions)
            {
                disposable.Dispose();
            }

            subscriptions.Clear();

            return base.StopAsync(cancellationToken);
        }
    }
}
