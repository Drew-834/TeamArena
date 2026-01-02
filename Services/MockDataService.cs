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
                }
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
    }
}