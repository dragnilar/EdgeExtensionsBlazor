using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorEdgeNewTab.Services;
using BlazorEdgeNewTab.Services.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEdgeNewTab;

public static class Program
{
#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        // workaround to use JavaScript fetch to bypass url validation
        // see: https://github.com/dotnet/runtime/issues/52836
        builder.Services.AddScoped<HttpClient>(sp => new JsHttpClient(sp)
            { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddBrowserExtensionServices();
        builder.Services.AddSingleton<INewTabService, NewTabService>();
        await builder.Build().RunAsync();
    }
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
}