using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using POT.Pexeso.Client.Providers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace POT.Pexeso.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddBlazoredToast();
            builder.Services.AddBlazoredLocalStorage(config => config.JsonSerializerOptions.WriteIndented = true);
            builder.Services.AddBlazoredModal();

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}
