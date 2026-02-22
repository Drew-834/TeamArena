using GameScoreboard.Server.Data;
using GameScoreboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameScoreboard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnapshotController : ControllerBase
{
    private readonly AppDbContext _db;
    public SnapshotController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<PodSnapshot>> Create([FromBody] PodSnapshot snapshot)
    {
        snapshot.SnapshotDate = DateTime.UtcNow;
        await _db.PodSnapshots.AddAsync(snapshot);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = snapshot.Id }, snapshot);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PodSnapshot>>> List([FromQuery] string? podName = null)
    {
        IQueryable<PodSnapshot> q = _db.PodSnapshots.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(podName))
            q = q.Where(s => s.PodName == podName);
        var results = await q.OrderByDescending(s => s.SnapshotDate).ToListAsync();
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PodSnapshot>> GetById(int id)
    {
        var snapshot = await _db.PodSnapshots.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return snapshot is null ? NotFound() : Ok(snapshot);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var snapshot = await _db.PodSnapshots.FindAsync(id);
        if (snapshot is null) return NotFound();
        _db.PodSnapshots.Remove(snapshot);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
