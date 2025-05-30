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
    .MinimumLevel.Debug()
    .WriteTo.BrowserConsole()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.Services.AddMudServices();
builder.Services.AddScoped<ContentService>();
builder.Services.AddSingleton<ContentContext>();
builder.Services.AddScoped<Similarity>();
builder.Services.AddScoped<ContextService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Blazored Local Storage services
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton<AudienceContextService>();

await builder.Build().RunAsync();
