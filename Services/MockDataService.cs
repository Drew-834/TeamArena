using GameScoreboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameScoreboard.Services
{
    public interface IDataService
    {
        Task<List<TeamMember>> GetTeamMembersAsync();
        Task<TeamMember> GetTeamMemberByIdAsync(int id);
    }

    public class MockDataService : IDataService
    {
        private readonly List<TeamMember> _teamMembers;

        public MockDataService()
        {
            // Initialize with mock data
            _teamMembers = new List<TeamMember>
            {
                new TeamMember
                {
                    Id = 1,
                    Name = "Alex Johnson",
                    AvatarUrl = "/images/avatars/avatar1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 85.4 },
                        { "GSP", 67.2 },
                        { "Revenue", 12450.75 },
                        { "ASP", 435.22 },
                        { "Basket", 3.8 },
                        { "PMAttach", 42.5 }
                    }
                },
                new TeamMember
                {
                    Id = 2,
                    Name = "Taylor Smith",
                    AvatarUrl = "/images/avatars/avatar2.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 62.8 },
                        { "GSP", 94.5 },
                        { "Revenue", 10275.50 },
                        { "ASP", 512.88 },
                        { "Basket", 2.5 },
                        { "PMAttach", 38.2 }
                    }
                },
                new TeamMember
                {
                    Id = 3,
                    Name = "Morgan Wong",
                    AvatarUrl = "/images/avatars/avatar3.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 73.1 },
                        { "GSP", 81.6 },
                        { "Revenue", 15820.25 },
                        { "ASP", 659.18 },
                        { "Basket", 4.2 },
                        { "PMAttach", 56.7 }
                    }
                },
                new TeamMember
                {
                    Id = 4,
                    Name = "Casey Martinez",
                    AvatarUrl = "/images/avatars/avatar4.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 92.3 },
                        { "GSP", 74.8 },
                        { "Revenue", 9875.50 },
                        { "ASP", 395.02 },
                        { "Basket", 3.1 },
                        { "PMAttach", 47.3 }
                    }
                },
                new TeamMember
                {
                    Id = 5,
                    Name = "Riley Zhang",
                    AvatarUrl = "/images/avatars/avatar5.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 79.5 },
                        { "GSP", 86.3 },
                        { "Revenue", 11950.75 },
                        { "ASP", 478.03 },
                        { "Basket", 5.2 },
                        { "PMAttach", 51.8 }
                    }
                },
                new TeamMember
                {
                    Id = 6,
                    Name = "Jordan Wilson",
                    AvatarUrl = "/images/avatars/avatar6.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 68.7 },
                        { "GSP", 79.2 },
                        { "Revenue", 14320.00 },
                        { "ASP", 596.67 },
                        { "Basket", 3.9 },
                        { "PMAttach", 63.4 }
                    }
                }
            };
        }

        public Task<List<TeamMember>> GetTeamMembersAsync()
        {
            return Task.FromResult(_teamMembers);
        }

        public Task<TeamMember> GetTeamMemberByIdAsync(int id)
        {
            return Task.FromResult(_teamMembers.FirstOrDefault(m => m.Id == id));
        }
    }
}