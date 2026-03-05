using GameScoreboard.Server.Data;
using GameScoreboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
        try
        {
            IQueryable<TeamMember> query = _db.TeamMembers
                .Include(m => m.MetricRecords)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(m => m.Department == department);

            var result = await query.ToListAsync();

            if (result.Any())
            {
                var allPeriods = result
                    .SelectMany(m => m.MetricRecords)
                    .Select(r => r.Period)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Distinct()
                    .ToList();

                var latestPeriod = GetLatestPeriod(allPeriods);
                Console.WriteLine($"MembersController: Found {result.Count} members, {allPeriods.Count} distinct periods, latest={latestPeriod ?? "null"}");

                if (!string.IsNullOrEmpty(latestPeriod))
                {
                    foreach (var member in result)
                    {
                        member.MetricRecords = member.MetricRecords
                            .Where(r => r.Period == latestPeriod)
                            .ToList();
                    }
                }
            }
            else
            {
                Console.WriteLine("MembersController: No members found in database.");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MembersController.Get error: {ex.Message}");
            return Ok(new List<TeamMember>());
        }
    }

    private static string? GetLatestPeriod(IEnumerable<string> periods)
    {
        return periods
            .Select(p => new { Period = p, EndDate = ParsePeriodEndDate(p) })
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefault()?.Period;
    }

    private static DateTime ParsePeriodEndDate(string period)
    {
        try
        {
            var parts = period.Split('-');
            if (parts.Length < 2) return DateTime.MinValue;
            var monthStartDate = DateTime.ParseExact(
                "01-" + parts[1], "dd-MMM yyyy", CultureInfo.InvariantCulture);
            return parts[0].Equals("Mid", StringComparison.OrdinalIgnoreCase)
                ? monthStartDate.AddDays(14)
                : monthStartDate.AddMonths(1).AddDays(-1);
        }
        catch { return DateTime.MinValue; }
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

    [HttpPost]
    public async Task<ActionResult<TeamMember>> Create(TeamMember member)
    {
        _db.TeamMembers.Add(member);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.TeamMembers.FindAsync(id);
        if (member is null) return NotFound();
        _db.TeamMembers.Remove(member);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("departments")]
    public async Task<ActionResult<IEnumerable<string>>> GetDepartments()
    {
        var departments = await _db.TeamMembers
            .Select(m => m.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
        return Ok(departments);
    }
} 