using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameScoreboard;
using GameScoreboard.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for potential future API integration
// This uses builder.HostEnvironment.BaseAddress which should respect <base href>
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the data service for dependency injection
builder.Services.AddSingleton<IDataService, MockDataService>();

var app = builder.Build();

// In Blazor WASM standalone, middleware like UsePathBase isn't configured here.
// The combination of <base href>, launchUrl, and setting AppBasePath should handle it.

await app.RunAsync();