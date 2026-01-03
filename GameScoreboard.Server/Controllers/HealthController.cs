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
        try
        {
            // Try to connect to database
            var canConnect = await _db.Database.CanConnectAsync();
            var memberCount = canConnect ? await _db.TeamMembers.CountAsync() : -1;
            
            return Ok(new { 
                canConnect, 
                memberCount,
                hasConnectionString = !string.IsNullOrEmpty(_config.GetConnectionString("DefaultConnection")),
                provider = _db.Database.ProviderName
            });
        }
        catch (Exception ex)
        {
            return Ok(new { 
                canConnect = false, 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                hasConnectionString = !string.IsNullOrEmpty(_config.GetConnectionString("DefaultConnection"))
            });
        }
    }
} 