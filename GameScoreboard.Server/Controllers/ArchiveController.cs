using GameScoreboard.Server.Data;
using GameScoreboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameScoreboard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchiveController : ControllerBase
{
    private readonly AppDbContext _db;
    public ArchiveController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Archive([FromBody] ArchivedPeriod model)
    {
        // Basic uniqueness ensured by DB unique index
        await _db.ArchivedPeriods.AddAsync(model);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArchivedPeriod>>> List([FromQuery] string? department = null)
    {
        IQueryable<ArchivedPeriod> q = _db.ArchivedPeriods.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(department))
            q = q.Where(a => a.Department == department);
        return Ok(await q.OrderByDescending(a => a.ArchivedAt).ToListAsync());
    }
} 