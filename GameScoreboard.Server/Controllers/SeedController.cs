using GameScoreboard.Server.Data;
using GameScoreboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameScoreboard.Server.Controllers;

/// <summary>
/// Admin endpoint to seed the database with initial data.
/// Protected by a simple admin key for now.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public SeedController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> SeedDatabase([FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        // Simple admin key protection - set this in Azure App Service Configuration
        var expectedKey = _configuration["AdminKey"] ?? "TeamArena2025!";
        if (adminKey != expectedKey)
        {
            return Unauthorized("Invalid admin key");
        }

        // Check if already seeded
        if (await _db.TeamMembers.AnyAsync())
        {
            return BadRequest("Database already has data. Use DELETE /api/seed to clear first.");
        }

        var members = GetInitialTeamMembers();
        
        await _db.TeamMembers.AddRangeAsync(members);
        await _db.SaveChangesAsync();

        return Ok(new { Message = $"Successfully seeded {members.Count} team members with metric records" });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearDatabase([FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        var expectedKey = _configuration["AdminKey"] ?? "TeamArena2025!";
        if (adminKey != expectedKey)
        {
            return Unauthorized("Invalid admin key");
        }

        // Clear all data
        _db.MetricRecords.RemoveRange(_db.MetricRecords);
        _db.TeamMembers.RemoveRange(_db.TeamMembers);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Database cleared successfully" });
    }

    private List<TeamMember> GetInitialTeamMembers()
    {
        var period = "EOM-Dec 2025";
        var department = "Computers";

        return new List<TeamMember>
        {
            CreateMember(1, "Drew", department, "Intel Sales Lead", "images/avatars/drew1.png", period,
                new() { ["M365Attach"] = "85.3%", ["GSP"] = "45.2%", ["Revenue"] = "$125,340", ["ASP"] = "$892", ["Basket"] = "2.4", ["PMAttach"] = "62.1%", ["5Star"] = "94.5%", ["BP"] = "1250", ["PM"] = "89", ["Picks"] = "156", ["Accuracy"] = "98.2%", ["Awk"] = "12" }),
            CreateMember(2, "Jon", department, "Apple Pro", "images/avatars/jon1.png", period,
                new() { ["M365Attach"] = "78.5%", ["GSP"] = "52.1%", ["Revenue"] = "$98,750", ["ASP"] = "$1,245", ["Basket"] = "1.9", ["PMAttach"] = "55.8%", ["5Star"] = "91.2%", ["BP"] = "980", ["PM"] = "72", ["Picks"] = "134", ["Accuracy"] = "97.8%", ["Awk"] = "15" }),
            CreateMember(3, "Gustavo", department, "Gaming Champion", "images/avatars/gustavo1.png", period,
                new() { ["M365Attach"] = "72.1%", ["GSP"] = "48.9%", ["Revenue"] = "$87,420", ["ASP"] = "$756", ["Basket"] = "2.1", ["PMAttach"] = "48.3%", ["5Star"] = "88.9%", ["BP"] = "870", ["PM"] = "65", ["Picks"] = "142", ["Accuracy"] = "96.5%", ["Awk"] = "18" }),
            CreateMember(4, "Vinny", department, "Solutions Expert", "images/avatars/vinny1.png", period,
                new() { ["M365Attach"] = "91.2%", ["GSP"] = "38.5%", ["Revenue"] = "$145,890", ["ASP"] = "$1,120", ["Basket"] = "2.8", ["PMAttach"] = "71.4%", ["5Star"] = "96.8%", ["BP"] = "1450", ["PM"] = "98", ["Picks"] = "167", ["Accuracy"] = "99.1%", ["Awk"] = "8" }),
            CreateMember(5, "Ishack", department, "Build Master", "images/avatars/ishack1.png", period,
                new() { ["M365Attach"] = "82.7%", ["GSP"] = "55.3%", ["Revenue"] = "$112,560", ["ASP"] = "$945", ["Basket"] = "2.2", ["PMAttach"] = "58.9%", ["5Star"] = "92.3%", ["BP"] = "1125", ["PM"] = "81", ["Picks"] = "148", ["Accuracy"] = "97.3%", ["Awk"] = "14" }),
            CreateMember(6, "Kla", department, "Retail Ranger", "images/avatars/kla1.png", period,
                new() { ["M365Attach"] = "75.8%", ["GSP"] = "42.1%", ["Revenue"] = "$78,340", ["ASP"] = "$678", ["Basket"] = "1.8", ["PMAttach"] = "44.2%", ["5Star"] = "89.5%", ["BP"] = "780", ["PM"] = "58", ["Picks"] = "128", ["Accuracy"] = "95.8%", ["Awk"] = "21" }),
            CreateMember(7, "Matthew", department, "Premium Consultant", "images/avatars/matthew1.png", period,
                new() { ["M365Attach"] = "88.4%", ["GSP"] = "61.2%", ["Revenue"] = "$134,670", ["ASP"] = "$1,089", ["Basket"] = "2.6", ["PMAttach"] = "67.8%", ["5Star"] = "95.1%", ["BP"] = "1340", ["PM"] = "92", ["Picks"] = "159", ["Accuracy"] = "98.6%", ["Awk"] = "10" }),
            CreateMember(8, "Adam", department, "Tech Specialist", "images/avatars/adam1.png", period,
                new() { ["M365Attach"] = "68.9%", ["GSP"] = "35.8%", ["Revenue"] = "$65,420", ["ASP"] = "$612", ["Basket"] = "1.6", ["PMAttach"] = "38.5%", ["5Star"] = "86.2%", ["BP"] = "650", ["PM"] = "48", ["Picks"] = "112", ["Accuracy"] = "94.2%", ["Awk"] = "25" }),
            CreateMember(9, "Ruben", department, "Service Star", "images/avatars/ruben1.png", period,
                new() { ["M365Attach"] = "79.3%", ["GSP"] = "44.7%", ["Revenue"] = "$92,180", ["ASP"] = "$834", ["Basket"] = "2.0", ["PMAttach"] = "52.6%", ["5Star"] = "90.8%", ["BP"] = "920", ["PM"] = "69", ["Picks"] = "138", ["Accuracy"] = "96.9%", ["Awk"] = "16" }),
            CreateMember(10, "Carlos", department, "Computing Ace", "images/avatars/adam1.png", period,
                new() { ["M365Attach"] = "84.6%", ["GSP"] = "49.8%", ["Revenue"] = "$118,750", ["ASP"] = "$967", ["Basket"] = "2.3", ["PMAttach"] = "61.2%", ["5Star"] = "93.4%", ["BP"] = "1185", ["PM"] = "85", ["Picks"] = "152", ["Accuracy"] = "97.7%", ["Awk"] = "13" }),
            CreateMember(11, "Maria", department, "Customer Champion", "images/avatars/kla1.png", period,
                new() { ["M365Attach"] = "86.1%", ["GSP"] = "51.4%", ["Revenue"] = "$108,920", ["ASP"] = "$912", ["Basket"] = "2.5", ["PMAttach"] = "64.7%", ["5Star"] = "97.2%", ["BP"] = "1090", ["PM"] = "88", ["Picks"] = "145", ["Accuracy"] = "98.8%", ["Awk"] = "9" }),
            CreateMember(12, "Alex", department, "PC Builder Pro", "images/avatars/gustavo2.png", period,
                new() { ["M365Attach"] = "71.4%", ["GSP"] = "58.6%", ["Revenue"] = "$76,540", ["ASP"] = "$723", ["Basket"] = "1.7", ["PMAttach"] = "42.1%", ["5Star"] = "87.6%", ["BP"] = "765", ["PM"] = "54", ["Picks"] = "124", ["Accuracy"] = "95.1%", ["Awk"] = "22" }),
            CreateMember(13, "Sofia", department, "Sales Specialist", "images/avatars/kla1.png", period,
                new() { ["M365Attach"] = "80.8%", ["GSP"] = "46.3%", ["Revenue"] = "$95,670", ["ASP"] = "$856", ["Basket"] = "2.1", ["PMAttach"] = "56.4%", ["5Star"] = "91.9%", ["BP"] = "955", ["PM"] = "73", ["Picks"] = "140", ["Accuracy"] = "97.1%", ["Awk"] = "15" }),
            CreateMember(14, "Miguel", department, "Tech Advisor", "images/avatars/ishack2.png", period,
                new() { ["M365Attach"] = "74.2%", ["GSP"] = "40.9%", ["Revenue"] = "$82,310", ["ASP"] = "$701", ["Basket"] = "1.9", ["PMAttach"] = "47.8%", ["5Star"] = "88.3%", ["BP"] = "820", ["PM"] = "61", ["Picks"] = "131", ["Accuracy"] = "96.2%", ["Awk"] = "19" }),
            CreateMember(15, "Emma", department, "Solutions Pro", "images/avatars/kla1.png", period,
                new() { ["M365Attach"] = "89.7%", ["GSP"] = "53.8%", ["Revenue"] = "$128,450", ["ASP"] = "$1,034", ["Basket"] = "2.7", ["PMAttach"] = "69.3%", ["5Star"] = "96.1%", ["BP"] = "1280", ["PM"] = "94", ["Picks"] = "162", ["Accuracy"] = "98.9%", ["Awk"] = "7" }),
            CreateMember(16, "David", department, "Hardware Hero", "images/avatars/vinny2.png", period,
                new() { ["M365Attach"] = "77.5%", ["GSP"] = "47.2%", ["Revenue"] = "$89,870", ["ASP"] = "$789", ["Basket"] = "2.0", ["PMAttach"] = "53.1%", ["5Star"] = "90.4%", ["BP"] = "895", ["PM"] = "67", ["Picks"] = "136", ["Accuracy"] = "96.7%", ["Awk"] = "17" }),
            CreateMember(17, "Jessica", department, "Tech Enthusiast", "images/avatars/kla1.png", period,
                new() { ["M365Attach"] = "83.9%", ["GSP"] = "50.6%", ["Revenue"] = "$104,230", ["ASP"] = "$878", ["Basket"] = "2.4", ["PMAttach"] = "59.7%", ["5Star"] = "92.8%", ["BP"] = "1040", ["PM"] = "78", ["Picks"] = "149", ["Accuracy"] = "97.5%", ["Awk"] = "14" })
        };
    }

    private TeamMember CreateMember(int id, string name, string department, string role, string avatar, string period, Dictionary<string, string> metrics)
    {
        var member = new TeamMember
        {
            Name = name,
            Department = department,
            Role = role,
            AvatarUrl = avatar,
            TotalExperience = CalculateXP(metrics),
            MetricRecords = new List<MetricRecord>()
        };

        foreach (var kvp in metrics)
        {
            member.MetricRecords.Add(new MetricRecord
            {
                Period = period,
                MetricKey = kvp.Key,
                Value = kvp.Value
            });
        }

        return member;
    }

    private double CalculateXP(Dictionary<string, string> metrics)
    {
        double xp = 0;
        foreach (var kvp in metrics)
        {
            if (double.TryParse(kvp.Value.TrimEnd('%').Replace("$", "").Replace(",", ""), out double val))
            {
                xp += val;
            }
        }
        return xp / 10; // Scale down for XP
    }
}

