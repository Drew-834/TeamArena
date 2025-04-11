using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameScoreboard;
using GameScoreboard.Services;
using GameScoreboard.Data;
using Microsoft.EntityFrameworkCore;
// using OfficeOpenXml; // Keep commented out or remove
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure SQLite DbContext
builder.Services.AddDbContextFactory<AppDbContext>(options => 
    options.UseSqlite($"Data Source={nameof(AppDbContext)}.db"));

// Register HttpClient for potential future API integration
// This uses builder.HostEnvironment.BaseAddress which should respect <base href>
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the data service for dependency injection
// TODO: Replace MockDataService with EfDataService once implemented
builder.Services.AddSingleton<IDataService, MockDataService>(); // Keep Mock for now so app runs
// builder.Services.AddScoped<IDataService, EfDataService>(); // Future registration

// Register AppState as a Singleton
builder.Services.AddSingleton<AppState>();

var app = builder.Build();

// In Blazor WASM standalone, middleware like UsePathBase isn't configured here.
// The combination of <base href>, launchUrl, and setting AppBasePath should handle it.

await app.RunAsync();