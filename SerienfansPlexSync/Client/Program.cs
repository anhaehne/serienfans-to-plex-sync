using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Plex.Api;
using Plex.Api.Api;
using SerienfansPlexSync.Client.Authentication;
using SerienfansPlexSync.Client.Services;
using SerienfansPlexSync.Shared.Services;

namespace SerienfansPlexSync.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //await Task.Delay(5000);

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(
                sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            
            // Blazor interop
            builder.Services.AddBlazoredLocalStorage();
            
            // Authentication
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, PlexAuthenticationProvider>();

            // Services
            builder.Services.AddScoped<ITvShowService, TvShowService>();
            builder.Services.AddScoped<IPlexService, PlexService>();
            builder.Services.AddScoped<ISerienfansService, SerienfansService>();
            builder.Services.AddScoped<IFilterStateService, FilterStateService>();

            AddPlexClient(builder.Services);
            builder.Services.AddTransient<IPlexService, PlexService>();

            await builder.Build().RunAsync();
        }

        private static void AddPlexClient(IServiceCollection services)
        {
            var apiOptions = new ClientOptions
            {
                Product = "SerienfansToPlex",
                DeviceName = "Temp",
                ClientId = "Temp",
                Platform = "Web",
                Version = "v1",
            };
            services.AddSingleton(apiOptions);
            services.AddTransient<IPlexClient, PlexClient>();
            services.AddTransient<IApiService, ApiService>();
            services.AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>();
        }
    }
}