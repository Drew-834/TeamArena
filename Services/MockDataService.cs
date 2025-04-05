using GameScoreboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameScoreboard.Services
{
    public interface IDataService
    {
        Task<List<TeamMember>> GetTeamMembersAsync(string? department = null);
        Task<TeamMember?> GetTeamMemberByIdAsync(int id);
        Task AddTeamMemberAsync(TeamMember member);
        Task UpdateTeamMemberAsync(TeamMember member);
        Task DeleteTeamMemberAsync(int id);
    }

    public class MockDataService : IDataService
    {
        private readonly List<TeamMember> _teamMembers;

        public MockDataService()
        {
            _teamMembers = InitializeMockData();
        }

        private List<TeamMember> InitializeMockData()
        {
            return new List<TeamMember>
            {
                // --- Computer Department Members ---
                new TeamMember
                {
                    Id = 1,
                    Name = "Adam",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/adam1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 50.0 },
                        { "GSP", 5.3 },
                        { "Revenue", 25185.0 },
                        { "ASP", 810.0 },
                        { "Basket", 198.0 },
                        { "PMAttach", 6.7 }
                    }
                },
                new TeamMember
                {
                    Id = 2,
                    Name = "Ruben",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/ruben1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 42.9 },
                        { "GSP", 22.0 },
                        { "Revenue", 19456.0 },
                        { "ASP", 738.0 },
                        { "Basket", 170.0 },
                        { "PMAttach", 9.5 }
                    }
                },
                new TeamMember
                {
                    Id = 3,
                    Name = "Ishack",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/ishack2.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 21.7 },
                        { "GSP", 3.0 },
                        { "Revenue", 23740.0 },
                        { "ASP", 762.0 },
                        { "Basket", 241.0 },
                        { "PMAttach", 13.0 }
                    }
                },
                new TeamMember
                {
                    Id = 4,
                    Name = "Drew",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/drew1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 42.9 },
                        { "GSP", 5.0 },
                        { "Revenue", 21081.0 },
                        { "ASP", 712.0 },
                        { "Basket", 132.0 },
                        { "PMAttach", 19.0 }
                    }
                },
                new TeamMember
                {
                    Id = 5,
                    Name = "Vinny",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/vinny2.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 0.0 },
                        { "GSP", 0.0 },
                        { "Revenue", 9305.0 },
                        { "ASP", 472.0 },
                        { "Basket", 113.0 },
                        { "PMAttach", 31.3 }
                    }
                },
                new TeamMember
                {
                    Id = 6,
                    Name = "Matthew",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/matthew1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 16.7 },
                        { "GSP", 0.0 },
                        { "Revenue", 9296.0 },
                        { "ASP", 527.0 },
                        { "Basket", 125.0 },
                        { "PMAttach", 50.0 }
                    }
                },
                new TeamMember
                {
                    Id = 7,
                    Name = "Jonathan",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/jon1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 12.5 },
                        { "GSP", 10.0 },
                        { "Revenue", 8577.0 },
                        { "ASP", 842.0 },
                        { "Basket", 160.0 },
                        { "PMAttach", 62.5 }
                    }
                },
                new TeamMember
                {
                    Id = 8,
                    Name = "Gustavo G",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/gustavo1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 41.7 },
                        { "GSP", 0.0 },
                        { "Revenue", 7590.0 },
                        { "ASP", 497.0 },
                        { "Basket", 123.0 },
                        { "PMAttach", 0.0 }
                    }
                },
                new TeamMember
                {
                    Id = 9,
                    Name = "Felipe",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/avatar4.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 0.0 },
                        { "GSP", 5.0 },
                        { "Revenue", 7406.0 },
                        { "ASP", 347.0 },
                        { "Basket", 175.0 },
                        { "PMAttach", 13.3 }
                    }
                },
                new TeamMember
                {
                    Id = 10,
                    Name = "Klarensky",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/kla1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "M365Attach", 0.0 },
                        { "GSP", 0.0 },
                        { "Revenue", 7926.0 },
                        { "ASP", 361.0 },
                        { "Basket", 54.0 },
                        { "PMAttach", 0.0 }
                    }
                },
                // --- Store Department Members (Sample) ---
                new TeamMember
                {
                    Id = 11,
                    Name = "Sarah",
                    Department = "Store",
                    AvatarUrl = "images/avatars/avatar1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "Revenue", 150000.0 },
                        { "5Star", 4.8 },
                        { "GSP", 18.5 },
                        { "Basket", 85.50 }
                    }
                },
                new TeamMember
                {
                    Id = 12,
                    Name = "Mike",
                    Department = "Store",
                    AvatarUrl = "images/avatars/avatar2.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "Revenue", 120000.0 },
                        { "5Star", 4.5 },
                        { "GSP", 15.2 },
                        { "Basket", 75.00 }
                    }
                },
                // --- Front End Department Members (Sample) ---
                new TeamMember
                {
                    Id = 13,
                    Name = "Linda",
                    Department = "Front",
                    AvatarUrl = "images/avatars/avatar3.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "GSP", 25.0 },
                        { "BP", 120 },
                        { "PM", 85 },
                        { "5Star", 4.9 }
                    }
                },
                new TeamMember
                {
                    Id = 14,
                    Name = "David",
                    Department = "Front",
                    AvatarUrl = "images/avatars/avatar5.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "GSP", 22.1 },
                        { "BP", 95 },
                        { "PM", 70 },
                        { "5Star", 4.7 }
                    }
                },
                // --- Warehouse Department Members (Sample) ---
                new TeamMember
                {
                    Id = 15,
                    Name = "Carlos",
                    Department = "Warehouse",
                    AvatarUrl = "images/avatars/ishack1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "Picks", 405 },
                        { "Accuracy", 98.2 },
                        { "Awk", 0.5 },
                        { "Units", 1402 }
                    }
                },
                new TeamMember
                {
                    Id = 16,
                    Name = "Jessica",
                    Department = "Warehouse",
                    AvatarUrl = "images/avatars/vinny1.png",
                    Metrics = new Dictionary<string, object>
                    {
                        { "Picks", 382 },
                        { "Accuracy", 99.5 },
                        { "Awk", 0.7 },
                        { "Units", 1280 }
                    }
                }
            };
        }

        public Task<List<TeamMember>> GetTeamMembersAsync(string? department = null)
        {
            List<TeamMember> filteredMembers;
            if (string.IsNullOrEmpty(department))
            {
                filteredMembers = _teamMembers;
            }
            else
            {
                filteredMembers = _teamMembers.Where(m => 
                    m.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            return Task.FromResult(filteredMembers);
        }

        public Task<TeamMember?> GetTeamMemberByIdAsync(int id)
        {
            var member = _teamMembers.FirstOrDefault(m => m.Id == id);
            return Task.FromResult(member);
        }

        public Task AddTeamMemberAsync(TeamMember member)
        {
            member.Id = _teamMembers.Max(m => m.Id) + 1; // Simple ID generation
            _teamMembers.Add(member);
            return Task.CompletedTask;
            // TODO: Replace with API call for real persistence
        }

        public Task UpdateTeamMemberAsync(TeamMember member)
        {
            var index = _teamMembers.FindIndex(m => m.Id == member.Id);
            if (index != -1)
            {
                // In the mock service, the reference is already updated in memory
                // by the WeeklyTracker page. This method is a placeholder.
                 _teamMembers[index] = member; // Explicitly update the list reference if needed
                 Console.WriteLine($"MockDataService: Updated member {member.Name} (ID: {member.Id}) in memory.");
            }
            else
            {
                 Console.WriteLine($"MockDataService: Member with ID {member.Id} not found for update.");
                 // Handle error or potentially add if not found?
            }
            // TODO: Replace this in-memory update simulation with a real API call
            // Example: await _httpClient.PutAsJsonAsync($"/api/teammembers/{member.Id}", member);
            return Task.CompletedTask;
        }

        public Task DeleteTeamMemberAsync(int id)
        {
            _teamMembers.RemoveAll(m => m.Id == id);
            return Task.CompletedTask;
            // TODO: Replace with API call for real persistence
        }
    }
}