using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameScoreboard;
using GameScoreboard.Services;
using System;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient → API base URL via appsettings (fallback to local server for dev)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7157/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Production: Use HttpDataService to call Azure API
// Development: Uncomment MockDataService for local testing without API
#if DEBUG
// For local dev without API running, use MockDataService
builder.Services.AddScoped<IDataService, MockDataService>();
#else
// Production uses HTTP API to persist data to Azure SQL
builder.Services.AddScoped<IDataService, HttpDataService>();
#endif

// App state
builder.Services.AddSingleton<AppState>();

var app = builder.Build();

await app.RunAsync();
