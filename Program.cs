using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameScoreboard;
using GameScoreboard.Services;
using GameScoreboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// BUG B02: SQLite does NOT work in Blazor WebAssembly!
// This is only here for local testing - production uses HttpDataService with API
// TODO T05: Remove this once API is fully implemented
builder.Services.AddDbContextFactory<AppDbContext>(options => 
    options.UseSqlite($"Data Source={nameof(AppDbContext)}.db"));

// HttpClient → API base URL via appsettings (fallback to local server default)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7157/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// TODO: Switch to HttpDataService once API is implemented
// For now, using MockDataService for local testing
builder.Services.AddScoped<IDataService, MockDataService>();

// App state
builder.Services.AddSingleton<AppState>();

var app = builder.Build();

await app.RunAsync();
