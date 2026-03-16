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
            IQueryable<MetricRecord> metricQuery = _db.MetricRecords.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(department))
                metricQuery = metricQuery.Where(r => r.TeamMember!.Department == department);

            var allPeriods = await metricQuery
                .Select(r => r.Period)
                .Where(p => p != null && p != "")
                .Distinct()
                .ToListAsync();

            var latestPeriod = GetLatestPeriod(allPeriods);
            Console.WriteLine($"MembersController: {allPeriods.Count} distinct periods, latest={latestPeriod ?? "null"}");

            IQueryable<TeamMember> memberQuery = _db.TeamMembers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(department))
                memberQuery = memberQuery.Where(m => m.Department == department);

            if (!string.IsNullOrEmpty(latestPeriod))
            {
                memberQuery = memberQuery.Include(m => m.MetricRecords.Where(r => r.Period == latestPeriod));
            }

            var result = await memberQuery.ToListAsync();
            Console.WriteLine($"MembersController: Returning {result.Count} members");

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
            // New format: ISO date "yyyy-MM-dd"
            if (DateTime.TryParseExact(period, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var isoDate))
                return isoDate;

            // Legacy format: "Mid-Feb 2026" or "EOM-Feb 2026"
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
        var existing = await _db.TeamMembers.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = update.Name;
        existing.Department = update.Department;
        existing.Role = update.Role;
        existing.AvatarUrl = update.AvatarUrl;
        existing.TotalExperience = update.TotalExperience;
        existing.RphGoal = update.RphGoal;
        existing.AppEffGoal = update.AppEffGoal;
        existing.PmEffGoal = update.PmEffGoal;
        existing.WarrantyAttachGoal = update.WarrantyAttachGoal;
        existing.AccAttachGoal = update.AccAttachGoal;

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

    // Removes duplicate TeamMember rows (same normalized name), keeping the highest Id.
    // Also removes any member whose Name matches a known Department (pod-label-as-member).
    // MetricRecords on the deleted rows are cascade-deleted automatically.
    // Returns the count of deleted records.
    [HttpPost("cleanup-duplicates")]
    public async Task<ActionResult<int>> CleanupDuplicates()
    {
        try
        {
            var allMembers = await _db.TeamMembers.ToListAsync();

            // All known department/pod names (normalized)
            var allDepts = allMembers
                .Select(m => m.Department.Trim().ToLowerInvariant())
                .ToHashSet();

            // 1. Duplicate rows: same normalized name, keep highest Id
            var duplicateIds = allMembers
                .GroupBy(m => m.Name.Trim().ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(m => m.Id).Skip(1))
                .Select(m => m.Id)
                .ToHashSet();

            // 2. Pod-label-as-member: Name equals any known department/pod name
            var podLabelIds = allMembers
                .Where(m => !duplicateIds.Contains(m.Id)
                            && allDepts.Contains(m.Name.Trim().ToLowerInvariant()))
                .Select(m => m.Id)
                .ToHashSet();

            var allToDeleteIds = duplicateIds.Union(podLabelIds).ToList();

            if (!allToDeleteIds.Any())
                return Ok(0);

            var toDelete = await _db.TeamMembers
                .Where(m => allToDeleteIds.Contains(m.Id))
                .ToListAsync();

            _db.TeamMembers.RemoveRange(toDelete);
            await _db.SaveChangesAsync();

            Console.WriteLine($"MembersController.CleanupDuplicates: removed {duplicateIds.Count} duplicates + {podLabelIds.Count} pod-label records = {toDelete.Count} total.");
            return Ok(toDelete.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MembersController.CleanupDuplicates error: {ex.Message}");
            return Ok(0);
        }
    }
} 