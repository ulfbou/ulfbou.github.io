using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Serilog;
using Homepage;
using Homepage.Common.Helpers;
using Homepage.Common.Services;
using Homepage.Common.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Log.Logger = new LoggerConfiguration()

#if DEBUG
    .MinimumLevel.Override("Homepage", Serilog.Events.LogEventLevel.Debug)
#endif
    .MinimumLevel.Warning()
    .WriteTo.BrowserConsole()
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                .AddMudServices()
                .AddScoped<IMarkdownService, ContentMarkdownService>()
                .AddSingleton<ILocalStorageService, LocalStorageService>()
                .AddScoped<IUrlSynchronizationService, UrlSynchronizationService>()
                .AddScoped<IUserActivityService, UserActivityService>()
                .AddScoped<AudienceContextService>()
                .AddScoped<ContentMarkdownService>()
                .AddScoped<ContextService>()
                .AddSingleton<FilterService>()
                .AddScoped<ReadingProgressService>()
                .AddScoped<ScrollTrackerService>()
                .AddScoped<Similarity>()
                .AddScoped<ContentMarkdownService>()
                .AddSingleton<LocalStorageService>()
                .AddScoped<UrlSynchronizationService>()
                .AddScoped<UserActivityService>();

await builder.Build().RunAsync();
