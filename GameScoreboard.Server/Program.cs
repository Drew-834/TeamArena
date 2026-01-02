using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using GameScoreboard.Server.Data;

namespace GameScoreboard.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "GameScoreboard API", Version = "v1" });
        });

        // Get connection string - Azure App Service uses SQLAZURECONNSTR_ prefix
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Log warning but don't fail - allows health checks without DB
            Console.WriteLine("WARNING: No connection string found. Database features will be unavailable.");
        }

        // DbContext - only add if we have a connection string
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                opt.UseSqlServer(connectionString);
            }
            else
            {
                // Use in-memory for startup without DB (health checks still work)
                opt.UseInMemoryDatabase("FallbackDb");
            }
        });

        // CORS for WASM client (local dev + production)
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(
                    "http://localhost:5117",
                    "https://localhost:5117", 
                    "http://localhost:5172", 
                    "https://localhost:5172",
                    "https://drew-834.github.io",
                    "https://teamarena-client-start.azurestaticapps.net"  // Azure Static Web App
                )
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        var app = builder.Build();

        // Ensure database exists (only if using SQL Server)
        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                Console.WriteLine("Database connection successful, tables ensured.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
                // Don't crash - let the app start for health checks
            }
        }

        // Configure the HTTP request pipeline.
        app.UseCors();
        
        // Enable Swagger in all environments for easier debugging
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}

public record HealthResponse(string Status);
