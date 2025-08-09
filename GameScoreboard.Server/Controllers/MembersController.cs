using GameScoreboard.Server.Data;
using GameScoreboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameScoreboard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _db;
    public MembersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamMember>>> Get([FromQuery] string? department = null)
    {
        IQueryable<TeamMember> query = _db.TeamMembers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(m => m.Department == department);
        }
        var result = await query.ToListAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamMember>> GetById(int id)
    {
        var member = await _db.TeamMembers.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TeamMember update)
    {
        if (id != update.Id) return BadRequest();
        _db.Entry(update).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }
} 