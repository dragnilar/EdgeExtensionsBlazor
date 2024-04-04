using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorEdgeNewTab.Services;
using BlazorEdgeNewTab.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEdgeNewTab;

public static class Program
{
#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.UseBrowserExtension(browserExtension =>
        {
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
        });
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton<INewTabService, NewTabService>();
        await builder.Build().RunAsync();
    }
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
}