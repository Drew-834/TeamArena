using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameScoreboard.Server.Data;

namespace GameScoreboard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public HealthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });

    [HttpGet("db")]
    public async Task<IActionResult> CheckDb()
    {
        var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
        // Mask the password for security
        var maskedConnStr = System.Text.RegularExpressions.Regex.Replace(
            connStr, @"Password=[^;]+", "Password=***");
        
        try
        {
            // Force a real connection attempt with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await _db.Database.OpenConnectionAsync(cts.Token);
            var memberCount = await _db.TeamMembers.CountAsync(cts.Token);
            await _db.Database.CloseConnectionAsync();
            
            return Ok(new { 
                canConnect = true, 
                memberCount,
                connectionString = maskedConnStr,
                provider = _db.Database.ProviderName
            });
        }
        catch (Exception ex)
        {
            return Ok(new { 
                canConnect = false, 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                connectionString = maskedConnStr
            });
        }
    }
} 