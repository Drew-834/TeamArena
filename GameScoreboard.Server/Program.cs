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

        // DbContext
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

        // Ensure database exists (initial deployment convenience)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        app.UseCors();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}

public record HealthResponse(string Status);
