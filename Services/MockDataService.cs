using GameScoreboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameScoreboard.Data;

namespace GameScoreboard.Services
{
    // BUG B01: CRITICAL - All data is stored in-memory only!
    // Data is lost when the page is refreshed or browser is closed.
    // TODO T01: Implement real persistence using:
    //   - IndexedDB (via Blazored.LocalStorage or js interop)
    //   - Backend API with database
    //   - Or browser LocalStorage for simple cases
    
    public interface IDataService
    {
        Task<List<TeamMember>> GetTeamMembersAsync(string? department = null);
        Task<TeamMember?> GetTeamMemberByIdAsync(int id);
        Task AddTeamMemberAsync(TeamMember member);
        Task UpdateTeamMemberAsync(TeamMember member);
        Task DeleteTeamMemberAsync(int id);
        Task SaveMetricRecordsForPeriodAsync(int memberId, string period, List<MetricRecord> records);
        Task<List<MetricRecord>> GetMetricRecordsAsync(int? memberId = null, string? period = null);
        Task SavePodSnapshotAsync(GameScoreboard.Models.PodSnapshot snapshot);
        Task<List<GameScoreboard.Models.PodSnapshot>> GetPodSnapshotsAsync(string? podName = null);
        Task<GameScoreboard.Models.PodSnapshot?> GetPodSnapshotByIdAsync(int id);
    }

    public class MockDataService : IDataService
    {
        private readonly List<TeamMember> _teamMembers;
        // Use a compound key (MemberId, Period) to store records distinctly
        private readonly Dictionary<(int MemberId, string Period), List<MetricRecord>> _metricRecords = new();

        public MockDataService()
        {
            _teamMembers = InitializeMockData();
            // Pre-populate _metricRecords from the initialized mock data
            PrePopulateMetricRecords();
        }
        
        private void PrePopulateMetricRecords()
        {
            foreach (var member in _teamMembers)
            {
                if (member.MetricRecords != null && member.MetricRecords.Any())
                {
                    // Get the period from the first record (they should all be the same)
                    var period = member.MetricRecords.First().Period;
                    var key = (member.Id, period);
                    _metricRecords[key] = member.MetricRecords.ToList();
                }
            }
            Console.WriteLine($"MockDataService: Pre-populated {_metricRecords.Count} metric record sets from initial data.");
        }

        private List<TeamMember> InitializeMockData()
        {
            // EOM-Dec 2025 data loaded from actual metrics spreadsheet
            const string currentPeriod = "EOM-Dec 2025";
            const string podPeriod = "Mid-Feb 2026";
            
            return new List<TeamMember>
            {
                // --- Computer Department Members (EOM-Dec 2025 Data) ---
                new TeamMember
                {
                    Id = 1,
                    Name = "Adam Flores",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 1, Period = currentPeriod, MetricKey = "Revenue", Value = "139298" },
                        new MetricRecord { TeamMemberId = 1, Period = currentPeriod, MetricKey = "ASP", Value = "651" },
                        new MetricRecord { TeamMemberId = 1, Period = currentPeriod, MetricKey = "Basket", Value = "108" },
                        new MetricRecord { TeamMemberId = 1, Period = currentPeriod, MetricKey = "M365Attach", Value = "11.3" },
                        new MetricRecord { TeamMemberId = 1, Period = currentPeriod, MetricKey = "PMAttach", Value = "12.5" },
                        new MetricRecord { TeamMemberId = 1, Period = currentPeriod, MetricKey = "GSP", Value = "8.0" }
                    }
                },
                new TeamMember
                {
                    Id = 2,
                    Name = "Ishack Ishack",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/ishack2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 2, Period = currentPeriod, MetricKey = "Revenue", Value = "143703" },
                        new MetricRecord { TeamMemberId = 2, Period = currentPeriod, MetricKey = "ASP", Value = "709" },
                        new MetricRecord { TeamMemberId = 2, Period = currentPeriod, MetricKey = "Basket", Value = "151" },
                        new MetricRecord { TeamMemberId = 2, Period = currentPeriod, MetricKey = "M365Attach", Value = "15.7" },
                        new MetricRecord { TeamMemberId = 2, Period = currentPeriod, MetricKey = "PMAttach", Value = "43.3" },
                        new MetricRecord { TeamMemberId = 2, Period = currentPeriod, MetricKey = "GSP", Value = "5.6" }
                    }
                },
                new TeamMember
                {
                    Id = 3,
                    Name = "Gustavo Iciarte",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 3, Period = currentPeriod, MetricKey = "Revenue", Value = "112648" },
                        new MetricRecord { TeamMemberId = 3, Period = currentPeriod, MetricKey = "ASP", Value = "538" },
                        new MetricRecord { TeamMemberId = 3, Period = currentPeriod, MetricKey = "Basket", Value = "133" },
                        new MetricRecord { TeamMemberId = 3, Period = currentPeriod, MetricKey = "M365Attach", Value = "21.9" },
                        new MetricRecord { TeamMemberId = 3, Period = currentPeriod, MetricKey = "PMAttach", Value = "22.5" },
                        new MetricRecord { TeamMemberId = 3, Period = currentPeriod, MetricKey = "GSP", Value = "3.2" }
                    }
                },
                new TeamMember
                {
                    Id = 4,
                    Name = "Drew Carrillo",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/drew1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 4, Period = currentPeriod, MetricKey = "Revenue", Value = "81072" },
                        new MetricRecord { TeamMemberId = 4, Period = currentPeriod, MetricKey = "ASP", Value = "629" },
                        new MetricRecord { TeamMemberId = 4, Period = currentPeriod, MetricKey = "Basket", Value = "191" },
                        new MetricRecord { TeamMemberId = 4, Period = currentPeriod, MetricKey = "M365Attach", Value = "57.9" },
                        new MetricRecord { TeamMemberId = 4, Period = currentPeriod, MetricKey = "PMAttach", Value = "54.7" },
                        new MetricRecord { TeamMemberId = 4, Period = currentPeriod, MetricKey = "GSP", Value = "6.1" }
                    }
                },
                new TeamMember
                {
                    Id = 5,
                    Name = "Vinicius Domingues",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 5, Period = currentPeriod, MetricKey = "Revenue", Value = "97410" },
                        new MetricRecord { TeamMemberId = 5, Period = currentPeriod, MetricKey = "ASP", Value = "638" },
                        new MetricRecord { TeamMemberId = 5, Period = currentPeriod, MetricKey = "Basket", Value = "167" },
                        new MetricRecord { TeamMemberId = 5, Period = currentPeriod, MetricKey = "M365Attach", Value = "14.5" },
                        new MetricRecord { TeamMemberId = 5, Period = currentPeriod, MetricKey = "PMAttach", Value = "14.5" },
                        new MetricRecord { TeamMemberId = 5, Period = currentPeriod, MetricKey = "GSP", Value = "5.8" }
                    }
                },
                new TeamMember
                {
                    Id = 6,
                    Name = "Adrian Rambaran",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 6, Period = currentPeriod, MetricKey = "Revenue", Value = "94869" },
                        new MetricRecord { TeamMemberId = 6, Period = currentPeriod, MetricKey = "ASP", Value = "592" },
                        new MetricRecord { TeamMemberId = 6, Period = currentPeriod, MetricKey = "Basket", Value = "155" },
                        new MetricRecord { TeamMemberId = 6, Period = currentPeriod, MetricKey = "M365Attach", Value = "6.8" },
                        new MetricRecord { TeamMemberId = 6, Period = currentPeriod, MetricKey = "PMAttach", Value = "22.6" },
                        new MetricRecord { TeamMemberId = 6, Period = currentPeriod, MetricKey = "GSP", Value = "7.6" }
                    }
                },
                new TeamMember
                {
                    Id = 7,
                    Name = "Paola Garcia Parra",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 7, Period = currentPeriod, MetricKey = "Revenue", Value = "94277" },
                        new MetricRecord { TeamMemberId = 7, Period = currentPeriod, MetricKey = "ASP", Value = "572" },
                        new MetricRecord { TeamMemberId = 7, Period = currentPeriod, MetricKey = "Basket", Value = "157" },
                        new MetricRecord { TeamMemberId = 7, Period = currentPeriod, MetricKey = "M365Attach", Value = "7.7" },
                        new MetricRecord { TeamMemberId = 7, Period = currentPeriod, MetricKey = "PMAttach", Value = "9.4" },
                        new MetricRecord { TeamMemberId = 7, Period = currentPeriod, MetricKey = "GSP", Value = "2.1" }
                    }
                },
                new TeamMember
                {
                    Id = 8,
                    Name = "Ruben Torres B.",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/ruben1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 8, Period = currentPeriod, MetricKey = "Revenue", Value = "93822" },
                        new MetricRecord { TeamMemberId = 8, Period = currentPeriod, MetricKey = "ASP", Value = "842" },
                        new MetricRecord { TeamMemberId = 8, Period = currentPeriod, MetricKey = "Basket", Value = "140" },
                        new MetricRecord { TeamMemberId = 8, Period = currentPeriod, MetricKey = "M365Attach", Value = "24.7" },
                        new MetricRecord { TeamMemberId = 8, Period = currentPeriod, MetricKey = "PMAttach", Value = "38.8" },
                        new MetricRecord { TeamMemberId = 8, Period = currentPeriod, MetricKey = "GSP", Value = "5.4" }
                    }
                },
                new TeamMember
                {
                    Id = 9,
                    Name = "Chitnanjan Lall",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 9, Period = currentPeriod, MetricKey = "Revenue", Value = "88237" },
                        new MetricRecord { TeamMemberId = 9, Period = currentPeriod, MetricKey = "ASP", Value = "654" },
                        new MetricRecord { TeamMemberId = 9, Period = currentPeriod, MetricKey = "Basket", Value = "128" },
                        new MetricRecord { TeamMemberId = 9, Period = currentPeriod, MetricKey = "M365Attach", Value = "7.7" },
                        new MetricRecord { TeamMemberId = 9, Period = currentPeriod, MetricKey = "PMAttach", Value = "11.0" },
                        new MetricRecord { TeamMemberId = 9, Period = currentPeriod, MetricKey = "GSP", Value = "10.6" }
                    }
                },
                new TeamMember
                {
                    Id = 10,
                    Name = "Gabriel Vargas",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 10, Period = currentPeriod, MetricKey = "Revenue", Value = "55286" },
                        new MetricRecord { TeamMemberId = 10, Period = currentPeriod, MetricKey = "ASP", Value = "560" },
                        new MetricRecord { TeamMemberId = 10, Period = currentPeriod, MetricKey = "Basket", Value = "162" },
                        new MetricRecord { TeamMemberId = 10, Period = currentPeriod, MetricKey = "M365Attach", Value = "13.0" },
                        new MetricRecord { TeamMemberId = 10, Period = currentPeriod, MetricKey = "PMAttach", Value = "35.1" },
                        new MetricRecord { TeamMemberId = 10, Period = currentPeriod, MetricKey = "GSP", Value = "2.2" }
                    }
                },
                new TeamMember
                {
                    Id = 11,
                    Name = "Robert Velazquez",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 11, Period = currentPeriod, MetricKey = "Revenue", Value = "61919" },
                        new MetricRecord { TeamMemberId = 11, Period = currentPeriod, MetricKey = "ASP", Value = "518" },
                        new MetricRecord { TeamMemberId = 11, Period = currentPeriod, MetricKey = "Basket", Value = "158" },
                        new MetricRecord { TeamMemberId = 11, Period = currentPeriod, MetricKey = "M365Attach", Value = "11.7" },
                        new MetricRecord { TeamMemberId = 11, Period = currentPeriod, MetricKey = "PMAttach", Value = "5.2" },
                        new MetricRecord { TeamMemberId = 11, Period = currentPeriod, MetricKey = "GSP", Value = "6.4" }
                    }
                },
                new TeamMember
                {
                    Id = 12,
                    Name = "Clara Cominato",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 12, Period = currentPeriod, MetricKey = "Revenue", Value = "70887" },
                        new MetricRecord { TeamMemberId = 12, Period = currentPeriod, MetricKey = "ASP", Value = "572" },
                        new MetricRecord { TeamMemberId = 12, Period = currentPeriod, MetricKey = "Basket", Value = "190" },
                        new MetricRecord { TeamMemberId = 12, Period = currentPeriod, MetricKey = "M365Attach", Value = "8.7" },
                        new MetricRecord { TeamMemberId = 12, Period = currentPeriod, MetricKey = "PMAttach", Value = "8.7" },
                        new MetricRecord { TeamMemberId = 12, Period = currentPeriod, MetricKey = "GSP", Value = "15.0" }
                    }
                },
                new TeamMember
                {
                    Id = 13,
                    Name = "Soph Glorioso",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 13, Period = currentPeriod, MetricKey = "Revenue", Value = "54450" },
                        new MetricRecord { TeamMemberId = 13, Period = currentPeriod, MetricKey = "ASP", Value = "406" },
                        new MetricRecord { TeamMemberId = 13, Period = currentPeriod, MetricKey = "Basket", Value = "303" },
                        new MetricRecord { TeamMemberId = 13, Period = currentPeriod, MetricKey = "M365Attach", Value = "1.5" },
                        new MetricRecord { TeamMemberId = 13, Period = currentPeriod, MetricKey = "PMAttach", Value = "12.3" },
                        new MetricRecord { TeamMemberId = 13, Period = currentPeriod, MetricKey = "GSP", Value = "2.5" }
                    }
                },
                new TeamMember
                {
                    Id = 14,
                    Name = "Matthew Fernandez",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 14, Period = currentPeriod, MetricKey = "Revenue", Value = "54923" },
                        new MetricRecord { TeamMemberId = 14, Period = currentPeriod, MetricKey = "ASP", Value = "449" },
                        new MetricRecord { TeamMemberId = 14, Period = currentPeriod, MetricKey = "Basket", Value = "383" },
                        new MetricRecord { TeamMemberId = 14, Period = currentPeriod, MetricKey = "M365Attach", Value = "7.0" },
                        new MetricRecord { TeamMemberId = 14, Period = currentPeriod, MetricKey = "PMAttach", Value = "19.7" },
                        new MetricRecord { TeamMemberId = 14, Period = currentPeriod, MetricKey = "GSP", Value = "6.6" }
                    }
                },
                new TeamMember
                {
                    Id = 15,
                    Name = "Nicholas Oti",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 15, Period = currentPeriod, MetricKey = "Revenue", Value = "26416" },
                        new MetricRecord { TeamMemberId = 15, Period = currentPeriod, MetricKey = "ASP", Value = "722" },
                        new MetricRecord { TeamMemberId = 15, Period = currentPeriod, MetricKey = "Basket", Value = "132" },
                        new MetricRecord { TeamMemberId = 15, Period = currentPeriod, MetricKey = "M365Attach", Value = "35.7" },
                        new MetricRecord { TeamMemberId = 15, Period = currentPeriod, MetricKey = "PMAttach", Value = "35.7" },
                        new MetricRecord { TeamMemberId = 15, Period = currentPeriod, MetricKey = "GSP", Value = "0.0" }
                    }
                },
                new TeamMember
                {
                    Id = 16,
                    Name = "Anthony Furtney",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 16, Period = currentPeriod, MetricKey = "Revenue", Value = "26996" },
                        new MetricRecord { TeamMemberId = 16, Period = currentPeriod, MetricKey = "ASP", Value = "570" },
                        new MetricRecord { TeamMemberId = 16, Period = currentPeriod, MetricKey = "Basket", Value = "105" },
                        new MetricRecord { TeamMemberId = 16, Period = currentPeriod, MetricKey = "M365Attach", Value = "9.1" },
                        new MetricRecord { TeamMemberId = 16, Period = currentPeriod, MetricKey = "PMAttach", Value = "15.2" },
                        new MetricRecord { TeamMemberId = 16, Period = currentPeriod, MetricKey = "GSP", Value = "0.0" }
                    }
                },
                new TeamMember
                {
                    Id = 17,
                    Name = "Vinicius Scramin",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 17, Period = currentPeriod, MetricKey = "Revenue", Value = "46895" },
                        new MetricRecord { TeamMemberId = 17, Period = currentPeriod, MetricKey = "ASP", Value = "406" },
                        new MetricRecord { TeamMemberId = 17, Period = currentPeriod, MetricKey = "Basket", Value = "131" },
                        new MetricRecord { TeamMemberId = 17, Period = currentPeriod, MetricKey = "M365Attach", Value = "1.9" },
                        new MetricRecord { TeamMemberId = 17, Period = currentPeriod, MetricKey = "PMAttach", Value = "0.0" },
                        new MetricRecord { TeamMemberId = 17, Period = currentPeriod, MetricKey = "GSP", Value = "1.9" }
                    }
                },
                // --- Store Department Members (Sample - keeping for other departments) ---
                new TeamMember
                {
                    Id = 18,
                    Name = "Sarah",
                    Department = "Store",
                    AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 18, Period = currentPeriod, MetricKey = "Revenue", Value = "150000.0" },
                        new MetricRecord { TeamMemberId = 18, Period = currentPeriod, MetricKey = "5Star", Value = "4.8" },
                        new MetricRecord { TeamMemberId = 18, Period = currentPeriod, MetricKey = "GSP", Value = "18.5" },
                        new MetricRecord { TeamMemberId = 18, Period = currentPeriod, MetricKey = "Basket", Value = "85.50" }
                    }
                },
                new TeamMember
                {
                    Id = 19,
                    Name = "Mike",
                    Department = "Store",
                    AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 19, Period = currentPeriod, MetricKey = "Revenue", Value = "120000.0" },
                        new MetricRecord { TeamMemberId = 19, Period = currentPeriod, MetricKey = "5Star", Value = "4.5" },
                        new MetricRecord { TeamMemberId = 19, Period = currentPeriod, MetricKey = "GSP", Value = "15.2" },
                        new MetricRecord { TeamMemberId = 19, Period = currentPeriod, MetricKey = "Basket", Value = "75.00" }
                    }
                },
                // --- Front End Department Members (Sample) ---
                new TeamMember
                {
                    Id = 20,
                    Name = "Linda",
                    Department = "Front",
                    AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 20, Period = currentPeriod, MetricKey = "GSP", Value = "25.0" },
                        new MetricRecord { TeamMemberId = 20, Period = currentPeriod, MetricKey = "BP", Value = "120" },
                        new MetricRecord { TeamMemberId = 20, Period = currentPeriod, MetricKey = "PM", Value = "85" },
                        new MetricRecord { TeamMemberId = 20, Period = currentPeriod, MetricKey = "5Star", Value = "4.9" }
                    }
                },
                new TeamMember
                {
                    Id = 21,
                    Name = "David",
                    Department = "Front",
                    AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 21, Period = currentPeriod, MetricKey = "GSP", Value = "22.1" },
                        new MetricRecord { TeamMemberId = 21, Period = currentPeriod, MetricKey = "BP", Value = "95" },
                        new MetricRecord { TeamMemberId = 21, Period = currentPeriod, MetricKey = "PM", Value = "70" },
                        new MetricRecord { TeamMemberId = 21, Period = currentPeriod, MetricKey = "5Star", Value = "4.7" }
                    }
                },
                // --- Warehouse Department Members (Sample) ---
                new TeamMember
                {
                    Id = 22,
                    Name = "Carlos",
                    Department = "Warehouse",
                    AvatarUrl = "images/avatars/ishack2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 22, Period = currentPeriod, MetricKey = "Picks", Value = "405" },
                        new MetricRecord { TeamMemberId = 22, Period = currentPeriod, MetricKey = "Accuracy", Value = "98.2" },
                        new MetricRecord { TeamMemberId = 22, Period = currentPeriod, MetricKey = "Awk", Value = "0.5" },
                        new MetricRecord { TeamMemberId = 22, Period = currentPeriod, MetricKey = "Units", Value = "1402" }
                    }
                },
                new TeamMember
                {
                    Id = 23,
                    Name = "Jessica",
                    Department = "Warehouse",
                    AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { TeamMemberId = 23, Period = currentPeriod, MetricKey = "Picks", Value = "382" },
                        new MetricRecord { TeamMemberId = 23, Period = currentPeriod, MetricKey = "Accuracy", Value = "99.5" },
                        new MetricRecord { TeamMemberId = 23, Period = currentPeriod, MetricKey = "Awk", Value = "0.7" },
                        new MetricRecord { TeamMemberId = 23, Period = currentPeriod, MetricKey = "Units", Value = "1280" }
                    }
                },

                // ========== POD MEMBERS (Feb 2026 Pod Performance Data) ==========
                // Pod: Matt-Category Advisors
                new TeamMember { Id = 100, Name = "Maria Navas Davis", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 100, Period = podPeriod, MetricKey = "RPH", Value = "1088" },
                        new MetricRecord { TeamMemberId = 100, Period = podPeriod, MetricKey = "AppEff", Value = "15851" },
                        new MetricRecord { TeamMemberId = 100, Period = podPeriod, MetricKey = "PMEff", Value = "14376" },
                        new MetricRecord { TeamMemberId = 100, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 100, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "7.2" },
                        new MetricRecord { TeamMemberId = 100, Period = podPeriod, MetricKey = "AccAttach", Value = "4.1" }
                    }},
                new TeamMember { Id = 101, Name = "Jacqueline Soto", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 101, Period = podPeriod, MetricKey = "RPH", Value = "1518" },
                        new MetricRecord { TeamMemberId = 101, Period = podPeriod, MetricKey = "AppEff", Value = "9660" },
                        new MetricRecord { TeamMemberId = 101, Period = podPeriod, MetricKey = "PMEff", Value = "3925" },
                        new MetricRecord { TeamMemberId = 101, Period = podPeriod, MetricKey = "Surveys", Value = "4.3" },
                        new MetricRecord { TeamMemberId = 101, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "15.5" },
                        new MetricRecord { TeamMemberId = 101, Period = podPeriod, MetricKey = "AccAttach", Value = "1.8" }
                    }},
                new TeamMember { Id = 102, Name = "Johnathon King", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 102, Period = podPeriod, MetricKey = "RPH", Value = "1606" },
                        new MetricRecord { TeamMemberId = 102, Period = podPeriod, MetricKey = "AppEff", Value = "14265" },
                        new MetricRecord { TeamMemberId = 102, Period = podPeriod, MetricKey = "PMEff", Value = "2684" },
                        new MetricRecord { TeamMemberId = 102, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 102, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "19.4" },
                        new MetricRecord { TeamMemberId = 102, Period = podPeriod, MetricKey = "AccAttach", Value = "8.9" }
                    }},
                new TeamMember { Id = 103, Name = "Daniel Grove", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 103, Period = podPeriod, MetricKey = "RPH", Value = "1097" },
                        new MetricRecord { TeamMemberId = 103, Period = podPeriod, MetricKey = "AppEff", Value = "10137" },
                        new MetricRecord { TeamMemberId = 103, Period = podPeriod, MetricKey = "PMEff", Value = "5003" },
                        new MetricRecord { TeamMemberId = 103, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 103, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "4.2" },
                        new MetricRecord { TeamMemberId = 103, Period = podPeriod, MetricKey = "AccAttach", Value = "22.7" }
                    }},
                new TeamMember { Id = 104, Name = "David Schunk", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 104, Period = podPeriod, MetricKey = "RPH", Value = "1106" },
                        new MetricRecord { TeamMemberId = 104, Period = podPeriod, MetricKey = "AppEff", Value = "10501" },
                        new MetricRecord { TeamMemberId = 104, Period = podPeriod, MetricKey = "PMEff", Value = "60960" },
                        new MetricRecord { TeamMemberId = 104, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 104, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "15.0" },
                        new MetricRecord { TeamMemberId = 104, Period = podPeriod, MetricKey = "AccAttach", Value = "21.7" }
                    }},
                new TeamMember { Id = 105, Name = "Christian Nazario", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/ruben1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 105, Period = podPeriod, MetricKey = "RPH", Value = "929" },
                        new MetricRecord { TeamMemberId = 105, Period = podPeriod, MetricKey = "AppEff", Value = "6516" },
                        new MetricRecord { TeamMemberId = 105, Period = podPeriod, MetricKey = "PMEff", Value = "3614" },
                        new MetricRecord { TeamMemberId = 105, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 105, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "13.8" },
                        new MetricRecord { TeamMemberId = 105, Period = podPeriod, MetricKey = "AccAttach", Value = "12.2" }
                    }},
                new TeamMember { Id = 106, Name = "Christopher Santos", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/ishack2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 106, Period = podPeriod, MetricKey = "RPH", Value = "1192" },
                        new MetricRecord { TeamMemberId = 106, Period = podPeriod, MetricKey = "AppEff", Value = "58610" },
                        new MetricRecord { TeamMemberId = 106, Period = podPeriod, MetricKey = "PMEff", Value = "2493" },
                        new MetricRecord { TeamMemberId = 106, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 106, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "12.9" },
                        new MetricRecord { TeamMemberId = 106, Period = podPeriod, MetricKey = "AccAttach", Value = "29.7" }
                    }},
                new TeamMember { Id = 107, Name = "Gerardo Torres", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/drew1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 107, Period = podPeriod, MetricKey = "RPH", Value = "1197" },
                        new MetricRecord { TeamMemberId = 107, Period = podPeriod, MetricKey = "AppEff", Value = "25358" },
                        new MetricRecord { TeamMemberId = 107, Period = podPeriod, MetricKey = "PMEff", Value = "5316" },
                        new MetricRecord { TeamMemberId = 107, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 107, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "5.5" },
                        new MetricRecord { TeamMemberId = 107, Period = podPeriod, MetricKey = "AccAttach", Value = "40.1" }
                    }},
                new TeamMember { Id = 108, Name = "William Cochrane", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 108, Period = podPeriod, MetricKey = "RPH", Value = "1273" },
                        new MetricRecord { TeamMemberId = 108, Period = podPeriod, MetricKey = "AppEff", Value = "11030" },
                        new MetricRecord { TeamMemberId = 108, Period = podPeriod, MetricKey = "PMEff", Value = "2284" },
                        new MetricRecord { TeamMemberId = 108, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 108, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "3.4" },
                        new MetricRecord { TeamMemberId = 108, Period = podPeriod, MetricKey = "AccAttach", Value = "23.2" }
                    }},
                new TeamMember { Id = 109, Name = "Anthony Rivera", Department = "Matt-Category Advisors", AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 109, Period = podPeriod, MetricKey = "RPH", Value = "1050" },
                        new MetricRecord { TeamMemberId = 109, Period = podPeriod, MetricKey = "AppEff", Value = "18314" },
                        new MetricRecord { TeamMemberId = 109, Period = podPeriod, MetricKey = "PMEff", Value = "5076" },
                        new MetricRecord { TeamMemberId = 109, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 109, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "4.1" },
                        new MetricRecord { TeamMemberId = 109, Period = podPeriod, MetricKey = "AccAttach", Value = "24.4" }
                    }},

                // Pod: LUIS-DI/HT/Mobile
                new TeamMember { Id = 110, Name = "Gerardo Cruz", Department = "Luis the Beast", AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 110, Period = podPeriod, MetricKey = "RPH", Value = "926" },
                        new MetricRecord { TeamMemberId = 110, Period = podPeriod, MetricKey = "AppEff", Value = "12477" },
                        new MetricRecord { TeamMemberId = 110, Period = podPeriod, MetricKey = "PMEff", Value = "26168" },
                        new MetricRecord { TeamMemberId = 110, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 110, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "7.4" },
                        new MetricRecord { TeamMemberId = 110, Period = podPeriod, MetricKey = "AccAttach", Value = "17.7" }
                    }},
                new TeamMember { Id = 111, Name = "Danna Nunez", Department = "Luis the Beast", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 111, Period = podPeriod, MetricKey = "RPH", Value = "945" },
                        new MetricRecord { TeamMemberId = 111, Period = podPeriod, MetricKey = "AppEff", Value = "13347" },
                        new MetricRecord { TeamMemberId = 111, Period = podPeriod, MetricKey = "PMEff", Value = "5159" },
                        new MetricRecord { TeamMemberId = 111, Period = podPeriod, MetricKey = "Surveys", Value = "4" },
                        new MetricRecord { TeamMemberId = 111, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "13.0" },
                        new MetricRecord { TeamMemberId = 111, Period = podPeriod, MetricKey = "AccAttach", Value = "15.1" }
                    }},
                new TeamMember { Id = 112, Name = "Julian Muriel", Department = "Luis the Beast", AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 112, Period = podPeriod, MetricKey = "RPH", Value = "543" },
                        new MetricRecord { TeamMemberId = 112, Period = podPeriod, MetricKey = "AppEff", Value = "16060" },
                        new MetricRecord { TeamMemberId = 112, Period = podPeriod, MetricKey = "PMEff", Value = "18092" },
                        new MetricRecord { TeamMemberId = 112, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 112, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "22.4" },
                        new MetricRecord { TeamMemberId = 112, Period = podPeriod, MetricKey = "AccAttach", Value = "6.7" }
                    }},
                new TeamMember { Id = 113, Name = "Sebastian Alvarez", Department = "Luis the Beast", AvatarUrl = "images/avatars/ishack2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 113, Period = podPeriod, MetricKey = "RPH", Value = "982" },
                        new MetricRecord { TeamMemberId = 113, Period = podPeriod, MetricKey = "AppEff", Value = "55628" },
                        new MetricRecord { TeamMemberId = 113, Period = podPeriod, MetricKey = "PMEff", Value = "6825" },
                        new MetricRecord { TeamMemberId = 113, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 113, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "4.5" },
                        new MetricRecord { TeamMemberId = 113, Period = podPeriod, MetricKey = "AccAttach", Value = "22.3" }
                    }},
                new TeamMember { Id = 114, Name = "Jose Lopez", Department = "Luis the Beast", AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 114, Period = podPeriod, MetricKey = "RPH", Value = "637" },
                        new MetricRecord { TeamMemberId = 114, Period = podPeriod, MetricKey = "AppEff", Value = "5356" },
                        new MetricRecord { TeamMemberId = 114, Period = podPeriod, MetricKey = "PMEff", Value = "14236" },
                        new MetricRecord { TeamMemberId = 114, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 114, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "6.5" },
                        new MetricRecord { TeamMemberId = 114, Period = podPeriod, MetricKey = "AccAttach", Value = "19.4" }
                    }},
                new TeamMember { Id = 115, Name = "Daniel Chaparro", Department = "Luis the Beast", AvatarUrl = "images/avatars/ruben1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 115, Period = podPeriod, MetricKey = "RPH", Value = "952" },
                        new MetricRecord { TeamMemberId = 115, Period = podPeriod, MetricKey = "AppEff", Value = "10885" },
                        new MetricRecord { TeamMemberId = 115, Period = podPeriod, MetricKey = "PMEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 115, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 115, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "10.8" },
                        new MetricRecord { TeamMemberId = 115, Period = podPeriod, MetricKey = "AccAttach", Value = "31.4" }
                    }},
                new TeamMember { Id = 116, Name = "Celine Paul", Department = "Luis the Beast", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 116, Period = podPeriod, MetricKey = "RPH", Value = "1066" },
                        new MetricRecord { TeamMemberId = 116, Period = podPeriod, MetricKey = "AppEff", Value = "64158" },
                        new MetricRecord { TeamMemberId = 116, Period = podPeriod, MetricKey = "PMEff", Value = "27665" },
                        new MetricRecord { TeamMemberId = 116, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 116, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "7.1" },
                        new MetricRecord { TeamMemberId = 116, Period = podPeriod, MetricKey = "AccAttach", Value = "32.5" }
                    }},
                new TeamMember { Id = 117, Name = "Gabriel Gonzalez", Department = "Luis the Beast", AvatarUrl = "images/avatars/drew1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 117, Period = podPeriod, MetricKey = "RPH", Value = "1021" },
                        new MetricRecord { TeamMemberId = 117, Period = podPeriod, MetricKey = "AppEff", Value = "4327" },
                        new MetricRecord { TeamMemberId = 117, Period = podPeriod, MetricKey = "PMEff", Value = "30597" },
                        new MetricRecord { TeamMemberId = 117, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 117, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "5.7" },
                        new MetricRecord { TeamMemberId = 117, Period = podPeriod, MetricKey = "AccAttach", Value = "21.0" }
                    }},
                new TeamMember { Id = 118, Name = "Marcos Castro Torres", Department = "Luis the Beast", AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 118, Period = podPeriod, MetricKey = "RPH", Value = "358" },
                        new MetricRecord { TeamMemberId = 118, Period = podPeriod, MetricKey = "AppEff", Value = "5582" },
                        new MetricRecord { TeamMemberId = 118, Period = podPeriod, MetricKey = "PMEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 118, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 118, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "9.1" },
                        new MetricRecord { TeamMemberId = 118, Period = podPeriod, MetricKey = "AccAttach", Value = "31.3" }
                    }},
                new TeamMember { Id = 119, Name = "Yoseph Cardozo", Department = "Luis the Beast", AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 119, Period = podPeriod, MetricKey = "RPH", Value = "984" },
                        new MetricRecord { TeamMemberId = 119, Period = podPeriod, MetricKey = "AppEff", Value = "8145" },
                        new MetricRecord { TeamMemberId = 119, Period = podPeriod, MetricKey = "PMEff", Value = "27650" },
                        new MetricRecord { TeamMemberId = 119, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 119, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "1.2" },
                        new MetricRecord { TeamMemberId = 119, Period = podPeriod, MetricKey = "AccAttach", Value = "36.5" }
                    }},
                new TeamMember { Id = 120, Name = "Ibrahim Adam", Department = "Luis the Beast", AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 120, Period = podPeriod, MetricKey = "RPH", Value = "769" },
                        new MetricRecord { TeamMemberId = 120, Period = podPeriod, MetricKey = "AppEff", Value = "4903" },
                        new MetricRecord { TeamMemberId = 120, Period = podPeriod, MetricKey = "PMEff", Value = "5607" },
                        new MetricRecord { TeamMemberId = 120, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 120, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "16.9" },
                        new MetricRecord { TeamMemberId = 120, Period = podPeriod, MetricKey = "AccAttach", Value = "8.3" }
                    }},

                // Pod: Drew's Crew-Computing
                new TeamMember { Id = 130, Name = "Cesar Perez", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 130, Period = podPeriod, MetricKey = "RPH", Value = "780" },
                        new MetricRecord { TeamMemberId = 130, Period = podPeriod, MetricKey = "AppEff", Value = "10504" },
                        new MetricRecord { TeamMemberId = 130, Period = podPeriod, MetricKey = "PMEff", Value = "10275" },
                        new MetricRecord { TeamMemberId = 130, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 130, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "4.4" },
                        new MetricRecord { TeamMemberId = 130, Period = podPeriod, MetricKey = "AccAttach", Value = "28.9" }
                    }},
                new TeamMember { Id = 131, Name = "Joao Aguiar", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/ishack2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 131, Period = podPeriod, MetricKey = "RPH", Value = "857" },
                        new MetricRecord { TeamMemberId = 131, Period = podPeriod, MetricKey = "AppEff", Value = "17931" },
                        new MetricRecord { TeamMemberId = 131, Period = podPeriod, MetricKey = "PMEff", Value = "5851" },
                        new MetricRecord { TeamMemberId = 131, Period = podPeriod, MetricKey = "Surveys", Value = "4.7" },
                        new MetricRecord { TeamMemberId = 131, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "11.8" },
                        new MetricRecord { TeamMemberId = 131, Period = podPeriod, MetricKey = "AccAttach", Value = "27.6" }
                    }},
                new TeamMember { Id = 132, Name = "Seyquan Williams", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 132, Period = podPeriod, MetricKey = "RPH", Value = "902" },
                        new MetricRecord { TeamMemberId = 132, Period = podPeriod, MetricKey = "AppEff", Value = "12263" },
                        new MetricRecord { TeamMemberId = 132, Period = podPeriod, MetricKey = "PMEff", Value = "4638" },
                        new MetricRecord { TeamMemberId = 132, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 132, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "0.0" },
                        new MetricRecord { TeamMemberId = 132, Period = podPeriod, MetricKey = "AccAttach", Value = "15.6" }
                    }},
                new TeamMember { Id = 133, Name = "Liz Tejeda Moras", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 133, Period = podPeriod, MetricKey = "RPH", Value = "1018" },
                        new MetricRecord { TeamMemberId = 133, Period = podPeriod, MetricKey = "AppEff", Value = "14059" },
                        new MetricRecord { TeamMemberId = 133, Period = podPeriod, MetricKey = "PMEff", Value = "1283" },
                        new MetricRecord { TeamMemberId = 133, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 133, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "16.7" },
                        new MetricRecord { TeamMemberId = 133, Period = podPeriod, MetricKey = "AccAttach", Value = "17.4" }
                    }},
                new TeamMember { Id = 134, Name = "Joao Richa", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/ruben1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 134, Period = podPeriod, MetricKey = "RPH", Value = "858" },
                        new MetricRecord { TeamMemberId = 134, Period = podPeriod, MetricKey = "AppEff", Value = "11756" },
                        new MetricRecord { TeamMemberId = 134, Period = podPeriod, MetricKey = "PMEff", Value = "1802" },
                        new MetricRecord { TeamMemberId = 134, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 134, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "1.5" },
                        new MetricRecord { TeamMemberId = 134, Period = podPeriod, MetricKey = "AccAttach", Value = "17.2" }
                    }},
                new TeamMember { Id = 135, Name = "Victor Richa", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/drew1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 135, Period = podPeriod, MetricKey = "RPH", Value = "1178" },
                        new MetricRecord { TeamMemberId = 135, Period = podPeriod, MetricKey = "AppEff", Value = "17276" },
                        new MetricRecord { TeamMemberId = 135, Period = podPeriod, MetricKey = "PMEff", Value = "1501" },
                        new MetricRecord { TeamMemberId = 135, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 135, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "6.0" },
                        new MetricRecord { TeamMemberId = 135, Period = podPeriod, MetricKey = "AccAttach", Value = "19.4" }
                    }},
                new TeamMember { Id = 136, Name = "DJ Skelton", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 136, Period = podPeriod, MetricKey = "RPH", Value = "1223" },
                        new MetricRecord { TeamMemberId = 136, Period = podPeriod, MetricKey = "AppEff", Value = "39606" },
                        new MetricRecord { TeamMemberId = 136, Period = podPeriod, MetricKey = "PMEff", Value = "35338" },
                        new MetricRecord { TeamMemberId = 136, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 136, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "1.4" },
                        new MetricRecord { TeamMemberId = 136, Period = podPeriod, MetricKey = "AccAttach", Value = "20.3" }
                    }},
                new TeamMember { Id = 137, Name = "Jeremy Morales", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 137, Period = podPeriod, MetricKey = "RPH", Value = "967" },
                        new MetricRecord { TeamMemberId = 137, Period = podPeriod, MetricKey = "AppEff", Value = "17535" },
                        new MetricRecord { TeamMemberId = 137, Period = podPeriod, MetricKey = "PMEff", Value = "2553" },
                        new MetricRecord { TeamMemberId = 137, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 137, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "12.1" },
                        new MetricRecord { TeamMemberId = 137, Period = podPeriod, MetricKey = "AccAttach", Value = "23.1" }
                    }},
                new TeamMember { Id = 138, Name = "Yerik Palacios", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 138, Period = podPeriod, MetricKey = "RPH", Value = "900" },
                        new MetricRecord { TeamMemberId = 138, Period = podPeriod, MetricKey = "AppEff", Value = "7815" },
                        new MetricRecord { TeamMemberId = 138, Period = podPeriod, MetricKey = "PMEff", Value = "1609" },
                        new MetricRecord { TeamMemberId = 138, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 138, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "6.2" },
                        new MetricRecord { TeamMemberId = 138, Period = podPeriod, MetricKey = "AccAttach", Value = "14.0" }
                    }},
                new TeamMember { Id = 139, Name = "Dakota French", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 139, Period = podPeriod, MetricKey = "RPH", Value = "1188" },
                        new MetricRecord { TeamMemberId = 139, Period = podPeriod, MetricKey = "AppEff", Value = "16210" },
                        new MetricRecord { TeamMemberId = 139, Period = podPeriod, MetricKey = "PMEff", Value = "5707" },
                        new MetricRecord { TeamMemberId = 139, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 139, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "5.9" },
                        new MetricRecord { TeamMemberId = 139, Period = podPeriod, MetricKey = "AccAttach", Value = "17.4" }
                    }},
                new TeamMember { Id = 140, Name = "Jesus Nessy", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/ishack2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 140, Period = podPeriod, MetricKey = "RPH", Value = "1173" },
                        new MetricRecord { TeamMemberId = 140, Period = podPeriod, MetricKey = "AppEff", Value = "10267" },
                        new MetricRecord { TeamMemberId = 140, Period = podPeriod, MetricKey = "PMEff", Value = "3639" },
                        new MetricRecord { TeamMemberId = 140, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 140, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "5.2" },
                        new MetricRecord { TeamMemberId = 140, Period = podPeriod, MetricKey = "AccAttach", Value = "15.7" }
                    }},
                new TeamMember { Id = 141, Name = "Francisco Ramirez", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/ruben1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 141, Period = podPeriod, MetricKey = "RPH", Value = "1217" },
                        new MetricRecord { TeamMemberId = 141, Period = podPeriod, MetricKey = "AppEff", Value = "5455" },
                        new MetricRecord { TeamMemberId = 141, Period = podPeriod, MetricKey = "PMEff", Value = "3611" },
                        new MetricRecord { TeamMemberId = 141, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 141, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "6.6" },
                        new MetricRecord { TeamMemberId = 141, Period = podPeriod, MetricKey = "AccAttach", Value = "19.5" }
                    }},
                new TeamMember { Id = 142, Name = "Dillan Rawlings", Department = "Drews Crew-Computing", AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 142, Period = podPeriod, MetricKey = "RPH", Value = "463" },
                        new MetricRecord { TeamMemberId = 142, Period = podPeriod, MetricKey = "AppEff", Value = "9040" },
                        new MetricRecord { TeamMemberId = 142, Period = podPeriod, MetricKey = "PMEff", Value = "23971" },
                        new MetricRecord { TeamMemberId = 142, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 142, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "4.9" },
                        new MetricRecord { TeamMemberId = 142, Period = podPeriod, MetricKey = "AccAttach", Value = "16.3" }
                    }},

                // Pod: Front End
                new TeamMember { Id = 150, Name = "Betzaida Cotto Hernandez", Department = "Pod-Front End", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 150, Period = podPeriod, MetricKey = "RPH", Value = "607" },
                        new MetricRecord { TeamMemberId = 150, Period = podPeriod, MetricKey = "AppEff", Value = "16712" },
                        new MetricRecord { TeamMemberId = 150, Period = podPeriod, MetricKey = "PMEff", Value = "7804" },
                        new MetricRecord { TeamMemberId = 150, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 150, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "7.5" },
                        new MetricRecord { TeamMemberId = 150, Period = podPeriod, MetricKey = "AccAttach", Value = "7.1" }
                    }},
                new TeamMember { Id = 151, Name = "Liss Juarez", Department = "Pod-Front End", AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 151, Period = podPeriod, MetricKey = "RPH", Value = "1007" },
                        new MetricRecord { TeamMemberId = 151, Period = podPeriod, MetricKey = "AppEff", Value = "16657" },
                        new MetricRecord { TeamMemberId = 151, Period = podPeriod, MetricKey = "PMEff", Value = "14257" },
                        new MetricRecord { TeamMemberId = 151, Period = podPeriod, MetricKey = "Surveys", Value = "5" },
                        new MetricRecord { TeamMemberId = 151, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "4.0" },
                        new MetricRecord { TeamMemberId = 151, Period = podPeriod, MetricKey = "AccAttach", Value = "5.7" }
                    }},
                new TeamMember { Id = 152, Name = "Kate Martinez", Department = "Pod-Front End", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 152, Period = podPeriod, MetricKey = "RPH", Value = "664" },
                        new MetricRecord { TeamMemberId = 152, Period = podPeriod, MetricKey = "AppEff", Value = "9229" },
                        new MetricRecord { TeamMemberId = 152, Period = podPeriod, MetricKey = "PMEff", Value = "20592" },
                        new MetricRecord { TeamMemberId = 152, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 152, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "6.0" },
                        new MetricRecord { TeamMemberId = 152, Period = podPeriod, MetricKey = "AccAttach", Value = "6.8" }
                    }},
                new TeamMember { Id = 153, Name = "Rasheka Fray", Department = "Pod-Front End", AvatarUrl = "images/avatars/adam1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 153, Period = podPeriod, MetricKey = "RPH", Value = "693" },
                        new MetricRecord { TeamMemberId = 153, Period = podPeriod, MetricKey = "AppEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 153, Period = podPeriod, MetricKey = "PMEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 153, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 153, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "0.0" },
                        new MetricRecord { TeamMemberId = 153, Period = podPeriod, MetricKey = "AccAttach", Value = "27.8" }
                    }},
                new TeamMember { Id = 154, Name = "Matthew Soto Velez", Department = "Pod-Front End", AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 154, Period = podPeriod, MetricKey = "RPH", Value = "1549" },
                        new MetricRecord { TeamMemberId = 154, Period = podPeriod, MetricKey = "AppEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 154, Period = podPeriod, MetricKey = "PMEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 154, Period = podPeriod, MetricKey = "Surveys", Value = "1" },
                        new MetricRecord { TeamMemberId = 154, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "1.4" },
                        new MetricRecord { TeamMemberId = 154, Period = podPeriod, MetricKey = "AccAttach", Value = "24.6" }
                    }},
                new TeamMember { Id = 155, Name = "Alexa Arias", Department = "Pod-Front End", AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 155, Period = podPeriod, MetricKey = "RPH", Value = "933" },
                        new MetricRecord { TeamMemberId = 155, Period = podPeriod, MetricKey = "AppEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 155, Period = podPeriod, MetricKey = "PMEff", Value = "43982" },
                        new MetricRecord { TeamMemberId = 155, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 155, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "3.9" },
                        new MetricRecord { TeamMemberId = 155, Period = podPeriod, MetricKey = "AccAttach", Value = "3.9" }
                    }},
                new TeamMember { Id = 156, Name = "Elian Calderon", Department = "Pod-Front End", AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 156, Period = podPeriod, MetricKey = "RPH", Value = "1296" },
                        new MetricRecord { TeamMemberId = 156, Period = podPeriod, MetricKey = "AppEff", Value = "46504" },
                        new MetricRecord { TeamMemberId = 156, Period = podPeriod, MetricKey = "PMEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 156, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 156, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "0.0" },
                        new MetricRecord { TeamMemberId = 156, Period = podPeriod, MetricKey = "AccAttach", Value = "7.5" }
                    }},
                new TeamMember { Id = 157, Name = "Doo Lee", Department = "Pod-Front End", AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord> {
                        new MetricRecord { TeamMemberId = 157, Period = podPeriod, MetricKey = "RPH", Value = "334" },
                        new MetricRecord { TeamMemberId = 157, Period = podPeriod, MetricKey = "AppEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 157, Period = podPeriod, MetricKey = "PMEff", Value = "100000" },
                        new MetricRecord { TeamMemberId = 157, Period = podPeriod, MetricKey = "Surveys", Value = "0" },
                        new MetricRecord { TeamMemberId = 157, Period = podPeriod, MetricKey = "WarrantyAttach", Value = "6.8" },
                        new MetricRecord { TeamMemberId = 157, Period = podPeriod, MetricKey = "AccAttach", Value = "5.3" }
                    }}
            };
        }

        // Helper to find the most recent period string present in the saved records
        private string? GetLatestPeriodWithData()
        {
            if (!_metricRecords.Any()) return null;
            return _metricRecords.Keys
                .Select(key => key.Period)
                .Distinct()
                .OrderByDescending(p => GetPeriodEndDateFromString(p))
                .FirstOrDefault();
        }

        // Helper to parse period string 
        private DateTime GetPeriodEndDateFromString(string period)
        {
            try
            {
                string[] parts = period.Split('-');
                if (parts.Length != 2) return DateTime.MinValue;
                string monthYear = parts[1]; 
                DateTime monthStartDate = DateTime.ParseExact("01-" + monthYear, "dd-MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                if (parts[0].Equals("Mid", StringComparison.OrdinalIgnoreCase))
                {
                    return monthStartDate.AddDays(14); // Approx mid-month end
                }
                else // EOM
                {
                    return monthStartDate.AddMonths(1).AddDays(-1); // End of month
                }
            }
            catch { return DateTime.MinValue; }
        }

        public Task<List<TeamMember>> GetTeamMembersAsync(string? department = null)
        {
            Console.WriteLine($"GetTeamMembersAsync called with department: {department ?? "ALL"}");
            var filteredMembersSource = _teamMembers;
            if (!string.IsNullOrEmpty(department))
            {
                filteredMembersSource = _teamMembers.Where(m => m.Department.Equals(department, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Create a fresh list to return, copying members
            var membersToReturn = filteredMembersSource.Select(m => new TeamMember 
            { 
                Id = m.Id, 
                Name = m.Name, 
                Department = m.Department, 
                Role = m.Role,
                AvatarUrl = m.AvatarUrl,
                TotalExperience = m.TotalExperience,
                MetricRecords = new List<MetricRecord>() // Initialize with empty list
            }).ToList();

            // --- Populate Latest Metric Records --- 
            string? latestPeriod = GetLatestPeriodWithData();
            Console.WriteLine($"GetTeamMembersAsync: Latest period with data is {latestPeriod ?? "None"}");

            if (latestPeriod != null)
            {
                foreach (var member in membersToReturn) // Populate the copies
                {
                    if (_metricRecords.TryGetValue((member.Id, latestPeriod), out var records))
                    {
                        member.MetricRecords = records ?? new List<MetricRecord>();
                        Console.WriteLine($"  Populated {member.MetricRecords.Count} records for {member.Name} for period {latestPeriod}");
                    }
                    else
                    { 
                        // Ensure list is empty if no records for the latest period
                        member.MetricRecords = new List<MetricRecord>(); 
                        Console.WriteLine($"  No records found for {member.Name} for period {latestPeriod}");
                    }
                }
            }
            // If latestPeriod is null, the MetricRecords remain empty as initialized.
            // --- End Population ---
            
            Console.WriteLine($"GetTeamMembersAsync returning {membersToReturn.Count} members.");
            return Task.FromResult(membersToReturn);
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
            var existingMember = _teamMembers.FirstOrDefault(m => m.Id == member.Id);
            if (existingMember != null)
            {
                // Update TotalExperience primarily
                existingMember.TotalExperience = member.TotalExperience;
                
                // Update other basic info IF provided (less critical for this app)
                // existingMember.Name = member.Name;
                // existingMember.Department = member.Department;
                // existingMember.Role = member.Role;
                // existingMember.AvatarUrl = member.AvatarUrl;
                Console.WriteLine($"MockDataService: Updated XP for member ID {member.Id} to {existingMember.TotalExperience:F2}");
            }
            else 
            {
                Console.WriteLine($"MockDataService: Member ID {member.Id} not found for UpdateTeamMemberAsync.");
            }
            return Task.CompletedTask;
        }

        public Task DeleteTeamMemberAsync(int id)
        {
            _teamMembers.RemoveAll(m => m.Id == id);
            return Task.CompletedTask;
            // TODO: Replace with API call for real persistence
        }

        public Task SaveMetricRecordsForPeriodAsync(int memberId, string period, List<MetricRecord> records)
        {
            // Use the compound key for storage
            var key = (memberId, period);
            _metricRecords[key] = records; // Stores/overwrites records for this specific member and period

            // Corrected log to show total distinct periods stored or total records across all periods
            Console.WriteLine($"MockDataService: Saved/Updated {records.Count} metric records for member ID {memberId} for period '{period}'. Total distinct period entries: {_metricRecords.Count}");

            // --- REMOVED: Update MetricRecords on the TeamMember object in _teamMembers list --- 
            /*
            var memberToUpdate = _teamMembers.FirstOrDefault(m => m.Id == memberId);
            if (memberToUpdate != null)
            { 
                // Ensure the collection is initialized if it's somehow null (shouldn't happen with current init)
                memberToUpdate.MetricRecords ??= new List<MetricRecord>(); 
                
                // Cast to List<T> to use RemoveAll and AddRange
                if (memberToUpdate.MetricRecords is List<MetricRecord> metricList)
                {
                    // Remove any existing records for the same period from the TeamMember's list
                    metricList.RemoveAll(r => r.Period == period);
                    
                    // Add the new records to the TeamMember's list
                    metricList.AddRange(records);
                    Console.WriteLine($"MockDataService: Synchronized MetricRecords collection on TeamMember object ID {memberId} for period '{period}'.");
                }
                else
                {
                     // Fallback if it's not a List<T> (less efficient)
                    Console.WriteLine($"MockDataService: WARNING - MetricRecords collection is not a List<T>. Using slower update method for ID {memberId}.");
                    var recordsToRemove = memberToUpdate.MetricRecords.Where(r => r.Period == period).ToList();
                    foreach(var record in recordsToRemove) memberToUpdate.MetricRecords.Remove(record);
                    foreach(var record in records) memberToUpdate.MetricRecords.Add(record);
                }
            }
            else
            {
                Console.WriteLine($"MockDataService: WARNING - Could not find TeamMember ID {memberId} in _teamMembers list to synchronize MetricRecords collection.");
            }
            */

            return Task.CompletedTask;
        }

        public Task<List<MetricRecord>> GetMetricRecordsAsync(int? memberId = null, string? period = null)
        {
            // Start with all records from the dictionary values
            IEnumerable<MetricRecord> query = _metricRecords.Values.SelectMany(list => list);

            // --- Corrected Filtering Logic for Compound Key --- 
            
            if (memberId.HasValue && !string.IsNullOrEmpty(period))
            {
                // Filter by specific MemberId AND Period
                query = _metricRecords
                    .Where(kvp => kvp.Key.MemberId == memberId.Value && kvp.Key.Period.Equals(period, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(kvp => kvp.Value);
            }
            else if (memberId.HasValue)
            {
                // Filter by MemberId only (all periods for that member)
                 query = _metricRecords
                    .Where(kvp => kvp.Key.MemberId == memberId.Value)
                    .SelectMany(kvp => kvp.Value);
            }
            else if (!string.IsNullOrEmpty(period))
            {
                 // Filter by Period only (all members for that period)
                 query = _metricRecords
                    .Where(kvp => kvp.Key.Period.Equals(period, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(kvp => kvp.Value);
            }
            // else: No filters provided, query remains all records

            // Execute the query and return the result
            var filteredRecords = query.ToList();
            
            // Logging for debugging
            Console.WriteLine($"GetMetricRecordsAsync: memberId={memberId}, period={period}. Found {filteredRecords.Count} records.");

            return Task.FromResult(filteredRecords);
        }

        private readonly List<GameScoreboard.Models.PodSnapshot> _podSnapshots = new();
        private int _nextSnapshotId = 1;

        public Task SavePodSnapshotAsync(GameScoreboard.Models.PodSnapshot snapshot)
        {
            snapshot.Id = _nextSnapshotId++;
            snapshot.SnapshotDate = DateTime.UtcNow;
            _podSnapshots.Add(snapshot);
            return Task.CompletedTask;
        }

        public Task<List<GameScoreboard.Models.PodSnapshot>> GetPodSnapshotsAsync(string? podName = null)
        {
            IEnumerable<GameScoreboard.Models.PodSnapshot> q = _podSnapshots;
            if (!string.IsNullOrWhiteSpace(podName))
                q = q.Where(s => s.PodName == podName);
            return Task.FromResult(q.OrderByDescending(s => s.SnapshotDate).ToList());
        }

        public Task<GameScoreboard.Models.PodSnapshot?> GetPodSnapshotByIdAsync(int id)
        {
            return Task.FromResult(_podSnapshots.FirstOrDefault(s => s.Id == id));
        }
    }
}