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

    [HttpPost("pods")]
    public async Task<IActionResult> SeedPodData([FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        var expectedKey = _configuration["AdminKey"] ?? "TeamArena2025!";
        if (adminKey != expectedKey)
            return Unauthorized("Invalid admin key");

        var existingPodDepts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Matt-Category Advisors", "Luis the Beast", "Drews Crew-Computing", "Pod-Front End"
        };

        var alreadySeeded = await _db.TeamMembers
            .AnyAsync(m => existingPodDepts.Contains(m.Department));
        if (alreadySeeded)
            return BadRequest("Pod data already exists. DELETE /api/seed to clear first, or use the PodTracker page.");

        var podMembers = GetPodMembers();
        await _db.TeamMembers.AddRangeAsync(podMembers);
        await _db.SaveChangesAsync();

        return Ok(new { Message = $"Seeded {podMembers.Count} pod members with metric records" });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearDatabase([FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        var expectedKey = _configuration["AdminKey"] ?? "TeamArena2025!";
        if (adminKey != expectedKey)
        {
            return Unauthorized("Invalid admin key");
        }

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

    private List<TeamMember> GetPodMembers()
    {
        var period = "Mid-Feb 2026";

        return new List<TeamMember>
        {
            // Matt-Category Advisors
            CreatePod("Maria Navas Davis", "Matt-Category Advisors", period, 1088, 15851, 14376, 5.0, 7.2, 4.1),
            CreatePod("Jacqueline Soto", "Matt-Category Advisors", period, 1518, 9660, 3925, 4.3, 15.5, 1.8),
            CreatePod("Johnathon King", "Matt-Category Advisors", period, 1606, 14265, 2684, 5.0, 19.4, 8.9),
            CreatePod("Daniel Grove", "Matt-Category Advisors", period, 1097, 10137, 5003, 0, 4.2, 22.7),
            CreatePod("David Schunk", "Matt-Category Advisors", period, 1106, 10501, 60960, 5.0, 15.0, 21.7),
            CreatePod("Christian Nazario", "Matt-Category Advisors", period, 929, 6516, 3614, 5.0, 13.8, 12.2),
            CreatePod("Christopher Santos", "Matt-Category Advisors", period, 1192, 58610, 2493, 5.0, 12.9, 29.7),
            CreatePod("Gerardo Torres", "Matt-Category Advisors", period, 1197, 25358, 5316, 5.0, 5.5, 40.1),
            CreatePod("William Cochrane", "Matt-Category Advisors", period, 1273, 11030, 2284, 0, 3.4, 23.2),
            CreatePod("Anthony Rivera", "Matt-Category Advisors", period, 1050, 18314, 5076, 5.0, 4.1, 24.4),

            // LUIS-DI/HT/Mobile
            CreatePod("Gerardo Cruz", "Luis the Beast", period, 926, 12477, 26168, 0, 7.4, 17.7),
            CreatePod("Danna Nunez", "Luis the Beast", period, 945, 13347, 5159, 4.0, 13.0, 15.1),
            CreatePod("Julian Muriel", "Luis the Beast", period, 543, 16060, 18092, 0, 22.4, 6.7),
            CreatePod("Sebastian Alvarez", "Luis the Beast", period, 982, 55628, 6825, 5.0, 4.5, 22.3),
            CreatePod("Jose Lopez", "Luis the Beast", period, 637, 5356, 14236, 0, 6.5, 19.4),
            CreatePod("Daniel Chaparro", "Luis the Beast", period, 952, 10885, 100000, 0, 10.8, 31.4),
            CreatePod("Celine Paul", "Luis the Beast", period, 1066, 64158, 27665, 0, 7.1, 32.5),
            CreatePod("Gabriel Gonzalez", "Luis the Beast", period, 1021, 4327, 30597, 0, 5.7, 21.0),
            CreatePod("Marcos Castro Torres", "Luis the Beast", period, 358, 5582, 100000, 0, 9.1, 31.3),
            CreatePod("Yoseph Cardozo", "Luis the Beast", period, 984, 8145, 27650, 5.0, 1.2, 36.5),
            CreatePod("Ibrahim Adam", "Luis the Beast", period, 769, 4903, 5607, 5.0, 16.9, 8.3),

            // Drews Crew-Computing
            CreatePod("Cesar Perez", "Drews Crew-Computing", period, 780, 10504, 10275, 5.0, 4.4, 28.9),
            CreatePod("Joao Aguiar", "Drews Crew-Computing", period, 857, 17931, 5851, 4.7, 11.8, 27.6),
            CreatePod("Seyquan Williams", "Drews Crew-Computing", period, 902, 12263, 4638, 5.0, 0.0, 15.6),
            CreatePod("Liz Tejeda Moras", "Drews Crew-Computing", period, 1018, 14059, 1283, 0, 16.7, 17.4),
            CreatePod("Joao Richa", "Drews Crew-Computing", period, 858, 11756, 1802, 5.0, 1.5, 17.2),
            CreatePod("Victor Richa", "Drews Crew-Computing", period, 1178, 17276, 1501, 0, 6.0, 19.4),
            CreatePod("DJ Skelton", "Drews Crew-Computing", period, 1223, 39606, 35338, 0, 1.4, 20.3),
            CreatePod("Jeremy Morales", "Drews Crew-Computing", period, 967, 17535, 2553, 0, 12.1, 23.1),
            CreatePod("Yerik Palacios", "Drews Crew-Computing", period, 900, 7815, 1609, 5.0, 6.2, 14.0),
            CreatePod("Dakota French", "Drews Crew-Computing", period, 1188, 16210, 5707, 5.0, 5.9, 17.4),
            CreatePod("Jesus Nessy", "Drews Crew-Computing", period, 1173, 10267, 3639, 5.0, 5.2, 15.7),
            CreatePod("Francisco Ramirez", "Drews Crew-Computing", period, 1217, 5455, 3611, 5.0, 6.6, 19.5),
            CreatePod("Dillan Rawlings", "Drews Crew-Computing", period, 463, 9040, 23971, 0, 4.9, 16.3),

            // Pod-Front End
            CreatePod("Betzaida Cotto Hernandez", "Pod-Front End", period, 607, 16712, 7804, 5.0, 7.5, 7.1),
            CreatePod("Liss Juarez", "Pod-Front End", period, 1007, 16657, 14257, 5.0, 4.0, 5.7),
            CreatePod("Kate Martinez", "Pod-Front End", period, 664, 9229, 20592, 0, 6.0, 6.8),
            CreatePod("Rasheka Fray", "Pod-Front End", period, 693, 100000, 100000, 0, 0.0, 27.8),
            CreatePod("Matthew Soto Velez", "Pod-Front End", period, 1549, 100000, 100000, 1.0, 1.4, 24.6),
            CreatePod("Alexa Arias", "Pod-Front End", period, 933, 100000, 43982, 0, 3.9, 3.9),
            CreatePod("Elian Calderon", "Pod-Front End", period, 1296, 46504, 100000, 0, 0.0, 7.5),
            CreatePod("Doo Lee", "Pod-Front End", period, 334, 100000, 100000, 0, 6.8, 5.3),
        };
    }

    private TeamMember CreatePod(string name, string department, string period,
        double rph, double appEff, double pmEff, double surveys, double warranty, double accAttach)
    {
        var metrics = new Dictionary<string, string>
        {
            ["RPH"] = rph.ToString("F0"),
            ["AppEff"] = appEff.ToString("F0"),
            ["PMEff"] = pmEff.ToString("F0"),
            ["Surveys"] = surveys.ToString("F1"),
            ["WarrantyAttach"] = warranty.ToString("F1"),
            ["AccAttach"] = accAttach.ToString("F1")
        };
        return CreateMember(0, name, department, "Pod Member", "images/avatars/adam1.png", period, metrics);
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

