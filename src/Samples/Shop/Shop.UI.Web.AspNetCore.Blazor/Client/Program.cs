using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shop.UI.Web.AspNetCore.Blazor.Client
{
    public class Program
    {
        public static JsonSerializerOptions DefaultJsonSerializerOptions;

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddMudServices();

            DefaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            DefaultJsonSerializerOptions.Converters.Add(new TimeSpanConverter());

            await builder.Build().RunAsync();
        }
    }
}
