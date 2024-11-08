using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor.Services;

using Ulfbou.GitHub.IO;
using Ulfbou.GitHub.IO.Common.Helpers;
using Ulfbou.GitHub.IO.Common.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<ContextService>();
builder.Services.AddSingleton<ContentContext>();

/*builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); 
*/
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("UlfBou.GitHub.IO") });

await builder.Build().RunAsync();
