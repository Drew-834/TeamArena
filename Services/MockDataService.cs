using GameScoreboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameScoreboard.Data;

namespace GameScoreboard.Services
{
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
            // Don't pre-populate _metricRecords from mock data, let SaveMetrics do it
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
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "50.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "5.3" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "25185.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "810.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "198.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "6.7" }
                    }
                },
                new TeamMember
                {
                    Id = 2,
                    Name = "Ruben",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/ruben1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "42.9" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "22.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "19456.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "738.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "170.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "9.5" }
                    }
                },
                new TeamMember
                {
                    Id = 3,
                    Name = "Ishack",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/ishack2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "21.7" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "3.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "23740.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "762.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "241.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "13.0" }
                    }
                },
                new TeamMember
                {
                    Id = 4,
                    Name = "Drew",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/drew1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "42.9" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "5.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "21081.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "712.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "132.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "19.0" }
                    }
                },
                new TeamMember
                {
                    Id = 5,
                    Name = "Vinny",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/vinny2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "0.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "0.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "9305.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "472.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "113.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "31.3" }
                    }
                },
                new TeamMember
                {
                    Id = 6,
                    Name = "Matthew",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "16.7" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "0.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "9296.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "527.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "125.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "50.0" }
                    }
                },
                new TeamMember
                {
                    Id = 7,
                    Name = "Jonathan",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "12.5" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "10.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "8577.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "842.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "160.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "62.5" }
                    }
                },
                new TeamMember
                {
                    Id = 8,
                    Name = "Gustavo G",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/gustavo1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "41.7" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "0.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "7590.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "497.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "123.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "0.0" }
                    }
                },
                new TeamMember
                {
                    Id = 9,
                    Name = "Felipe",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/matthew1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "0.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "5.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "7406.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "347.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "175.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "13.3" }
                    }
                },
                new TeamMember
                {
                    Id = 10,
                    Name = "Klarensky",
                    Department = "Computers",
                    AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "M365Attach", Value = "0.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "0.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "7926.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "ASP", Value = "361.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "54.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PMAttach", Value = "0.0" }
                    }
                },
                // --- Store Department Members (Sample) ---
                new TeamMember
                {
                    Id = 11,
                    Name = "Sarah",
                    Department = "Store",
                    AvatarUrl = "images/avatars/avatar1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "150000.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "5Star", Value = "4.8" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "18.5" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "85.50" }
                    }
                },
                new TeamMember
                {
                    Id = 12,
                    Name = "Mike",
                    Department = "Store",
                    AvatarUrl = "images/avatars/avatar2.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Revenue", Value = "120000.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "5Star", Value = "4.5" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "15.2" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Basket", Value = "75.00" }
                    }
                },
                // --- Front End Department Members (Sample) ---
                new TeamMember
                {
                    Id = 13,
                    Name = "Linda",
                    Department = "Front",
                    AvatarUrl = "images/avatars/kla1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "25.0" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "BP", Value = "120" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PM", Value = "85" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "5Star", Value = "4.9" }
                    }
                },
                new TeamMember
                {
                    Id = 14,
                    Name = "David",
                    Department = "Front",
                    AvatarUrl = "images/avatars/jon1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "GSP", Value = "22.1" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "BP", Value = "95" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "PM", Value = "70" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "5Star", Value = "4.7" }
                    }
                },
                // --- Warehouse Department Members (Sample) ---
                new TeamMember
                {
                    Id = 15,
                    Name = "Carlos",
                    Department = "Warehouse",
                    AvatarUrl = "images/avatars/ishack1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Picks", Value = "405" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Accuracy", Value = "98.2" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Awk", Value = "0.5" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Units", Value = "1402" }
                    }
                },
                new TeamMember
                {
                    Id = 16,
                    Name = "Jessica",
                    Department = "Warehouse",
                    AvatarUrl = "images/avatars/vinny1.png",
                    MetricRecords = new List<MetricRecord>
                    {
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Picks", Value = "382" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Accuracy", Value = "99.5" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Awk", Value = "0.7" },
                        new MetricRecord { Period = "MockPeriod", MetricKey = "Units", Value = "1280" }
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