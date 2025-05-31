using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Serilog;
using Homepage;
using Homepage.Common.Helpers;
using Homepage.Common.Services;
using Homepage.Common.Models;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Homepage", Serilog.Events.LogEventLevel.Debug)
    .MinimumLevel.Warning()
    .WriteTo.BrowserConsole()
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                .AddBlazoredLocalStorage()
                .AddScoped<IMarkdownService, ContentMarkdownService>()
                .AddScoped<ContentMarkdownService>()
                .AddScoped<AudienceContextService>()
                .AddScoped<Similarity>()
                .AddScoped<ContextService>()
                .AddMudServices();

await builder.Build().RunAsync();
