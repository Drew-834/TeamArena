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

// Optional: keep SQLite for local testing (not used once API is live)
builder.Services.AddDbContextFactory<AppDbContext>(options => 
    options.UseSqlite($"Data Source={nameof(AppDbContext)}.db"));

// HttpClient → API base URL via appsettings (fallback to local server default)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7157/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Switch to HTTP data service
builder.Services.AddScoped<IDataService, HttpDataService>();

// App state
builder.Services.AddSingleton<AppState>();

var app = builder.Build();

await app.RunAsync();