using GameScoreboard.Server.Data;
using GameScoreboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameScoreboard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly AppDbContext _db;
    public MetricsController(AppDbContext db) => _db = db;

    [HttpPost("{memberId:int}/{period}")]
    public async Task<IActionResult> Save(int memberId, string period, [FromBody] List<MetricRecord> records)
    {
        var existing = _db.MetricRecords.Where(r => r.TeamMemberId == memberId && r.Period == period);
        _db.MetricRecords.RemoveRange(existing);
        foreach (var r in records)
        {
            r.TeamMemberId = memberId;
            r.Period = period;
        }
        await _db.MetricRecords.AddRangeAsync(records);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MetricRecord>>> Get([FromQuery] int? memberId = null, [FromQuery] string? period = null)
    {
        try
        {
            IQueryable<MetricRecord> q = _db.MetricRecords.AsNoTracking();
            if (memberId.HasValue) q = q.Where(r => r.TeamMemberId == memberId.Value);
            if (!string.IsNullOrWhiteSpace(period)) q = q.Where(r => r.Period == period);
            return Ok(await q.ToListAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MetricsController.Get error: {ex.Message}");
            return Ok(new List<MetricRecord>());
        }
    }
} 