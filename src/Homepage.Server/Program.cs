using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Homepage.Server.Data;
using Serilog;
using MudBlazor.Services;
using Homepage.Common.Services;
using Homepage.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddMudServices();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<ContextService>();
builder.Services.AddSingleton<ContentContext>();
builder.Services.AddScoped<ThemeGeneratorService>();
builder.Services.AddScoped<RandomizationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();