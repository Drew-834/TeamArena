using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using GameScoreboard.Server.Data;
using System.IO.Compression;

namespace GameScoreboard.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });
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
        var allowedCorsOrigins = new[]
        {
            "http://localhost:5117",
            "https://localhost:5117",
            "http://localhost:5172",
            "https://localhost:5172",
            "https://drew-834.github.io",
            "https://red-ocean-08c63b90f.1.azurestaticapps.net",
            "https://nice-dune-02544d51e.7.azurestaticapps.net",
            "https://teamarena-client-start.azurestaticapps.net"
        };
        builder.Services.AddSingleton(allowedCorsOrigins);
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(allowedCorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        builder.Services.AddResponseCompression(opts =>
        {
            opts.EnableForHttps = true;
            opts.Providers.Add<BrotliCompressionProvider>();
            opts.Providers.Add<GzipCompressionProvider>();
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(opts => opts.Level = CompressionLevel.Fastest);
        builder.Services.Configure<GzipCompressionProviderOptions>(opts => opts.Level = CompressionLevel.Fastest);

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

                if (db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
                {
                    try
                    {
                        db.Database.ExecuteSqlRaw("""
                            IF NOT EXISTS (SELECT 1 FROM sys.columns
                                WHERE object_id = OBJECT_ID(N'[TeamMembers]') AND name = N'CompanyRank')
                            ALTER TABLE [TeamMembers] ADD [CompanyRank] int NULL;
                            """);
                    }
                    catch (Exception sqlEx)
                    {
                        Console.WriteLine($"Schema patch CompanyRank (optional): {sqlEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
                // Don't crash - let the app start for health checks
            }
        }

        // Configure the HTTP request pipeline.
        app.UseResponseCompression();
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
