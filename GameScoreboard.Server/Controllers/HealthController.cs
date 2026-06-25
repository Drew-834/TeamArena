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
    private readonly string[] _allowedCorsOrigins;

    public HealthController(AppDbContext db, IConfiguration config, string[] allowedCorsOrigins)
    {
        _db = db;
        _config = config;
        _allowedCorsOrigins = allowedCorsOrigins;
    }

    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });

    [HttpGet("cors-debug")]
    public IActionResult CorsDebug()
    {
        var origin = Request.Headers.Origin.ToString();

        // #region agent log
        return Ok(new
        {
            sessionId = "e68a4d",
            runId = "cors-debug-pre-fix",
            hypothesisId = "H1,H2,H3,H5",
            origin,
            hasOriginHeader = !string.IsNullOrWhiteSpace(origin),
            allowedOrigins = _allowedCorsOrigins,
            originConfigured = _allowedCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase),
            timestamp = DateTimeOffset.UtcNow
        });
        // #endregion
    }

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