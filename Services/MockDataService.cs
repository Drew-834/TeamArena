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
                    Name = "Adam",
                    AvatarUrl = "images/avatars/adam1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 42.3 },
                        { "GSP", 14 },
                        { "Revenue", 21035 },
                        { "ASP", 578 },
                        { "Basket", 146 },
                        { "PMAttach", 15.4 }
                    }
                },
                new TeamMember
                {
                    Id = 2,
                    Name = "Ruben",
                    AvatarUrl = "images/avatars/ruben1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 42.9 },
                        { "GSP", 22 },
                        { "Revenue", 19456 },
                        { "ASP", 738 },
                        { "Basket", 170 },
                        { "PMAttach", 9.5 }
                    }
                },
                new TeamMember
                {
                    Id = 3,
                    Name = "Ishack",
                    AvatarUrl = "images/avatars/ishack2.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 21.7 },
                        { "GSP", 3 },
                        { "Revenue", 23740 },
                        { "ASP", 762 },
                        { "Basket", 241 },
                        { "PMAttach", 13 }
                    }
                },
                new TeamMember
                {
                    Id = 4,
                    Name = "Drew",
                    AvatarUrl = "images/avatars/drew1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 42.9 },
                        { "GSP", 5 },
                        { "Revenue", 21081 },
                        { "ASP", 712 },
                        { "Basket", 132 },
                        { "PMAttach", 19 }
                    }
                },
                new TeamMember
                {
                    Id = 5,
                    Name = "Vinny",
                    AvatarUrl = "images/avatars/vinny2.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 0 },
                        { "GSP", 0 },
                        { "Revenue", 9305 },
                        { "ASP", 472 },
                        { "Basket", 113 },
                        { "PMAttach", 31.3 }
                    }
                },
                new TeamMember
                {
                    Id = 6,
                    Name = "Matthew",
                    AvatarUrl = "images/avatars/matthew1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 16.7 },
                        { "GSP", 0 },
                        { "Revenue", 9296 },
                        { "ASP", 527 },
                        { "Basket", 125 },
                        { "PMAttach", 50 }
                    }

                },
                 new TeamMember
                {
                    Id = 7,
                    Name = "Jonathan",
                    AvatarUrl = "images/avatars/jon1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 12.5 },
                        { "GSP", 10 },
                        { "Revenue", 8577 },
                        { "ASP", 842 },
                        { "Basket", 160 },
                        { "PMAttach", 62.5 }
                    }
                },
                  new TeamMember
                {
                    Id = 8,
                    Name = "Gustavo G",
                    AvatarUrl = "images/avatars/gustavo1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 41.7 },
                        { "GSP", 0 },
                        { "Revenue", 7590 },
                        { "ASP", 497 },
                        { "Basket", 123 },
                        { "PMAttach", 0 }
                    }
                },
                   new TeamMember
                {
                    Id = 9,
                    Name = "Felipe",
                    AvatarUrl = "images/avatars/avatar4.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 0 },
                        { "GSP", 5 },
                        { "Revenue", 7406 },
                        { "ASP", 347 },
                        { "Basket", 175 },
                        { "PMAttach", 13.3 }
                    }
                },
                    new TeamMember
                {
                    Id = 10,
                    Name = "Klarensky",
                    AvatarUrl = "images/avatars/kla1.png",
                    Metrics = new Dictionary<string, double>
                    {
                        { "M365Attach", 0 },
                        { "GSP", 0 },
                        { "Revenue", 7926 },
                        { "ASP", 361 },
                        { "Basket", 54 },
                        { "PMAttach", 0 }
                    }
                },
            };
        }

        public Task<List<TeamMember>> GetTeamMembersAsync()
        {
            return Task.FromResult(_teamMembers);
        }

        public Task<TeamMember> GetTeamMemberByIdAsync(int id)
        {
            return Task.FromResult(_teamMembers.First(m => m.Id == id));
        }
    }
}