﻿using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorLiquidSeperation
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            // workaround to use JavaScript fetch to bypass url validation
            // see: https://github.com/dotnet/runtime/issues/52836
            builder.Services.AddScoped<HttpClient>(sp => new JsHttpClient(sp) { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddBrowserExtensionServices(options =>
            {
                options.ProjectNamespace = typeof(Program).Namespace;
            });
            await builder.Build().RunAsync();
        }
    }
}
