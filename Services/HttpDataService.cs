using GameScoreboard.Models;
using GameScoreboard.Data;
using System.Net.Http.Json;

namespace GameScoreboard.Services
{
    /// <summary>
    /// HTTP-based data service that communicates with the backend API.
    /// Used in production to persist data to Azure SQL via the API.
    /// </summary>
    public class HttpDataService : IDataService
    {
        private readonly HttpClient _http;

        public HttpDataService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TeamMember>> GetTeamMembersAsync(string? department = null)
        {
            try
            {
                var url = "api/members";
                if (!string.IsNullOrWhiteSpace(department))
                {
                    url += $"?department={Uri.EscapeDataString(department)}";
                }

                var members = await _http.GetFromJsonAsync<List<TeamMember>>(url);
                
                if (members != null && members.Any())
                {
                    // Fetch metric records for each member and populate
                    var allRecords = await GetMetricRecordsAsync();
                    var latestPeriod = GetLatestPeriod(allRecords.Select(r => r.Period).Distinct());
                    
                    if (!string.IsNullOrEmpty(latestPeriod))
                    {
                        foreach (var member in members)
                        {
                            member.MetricRecords = allRecords
                                .Where(r => r.TeamMemberId == member.Id && r.Period == latestPeriod)
                                .ToList();
                        }
                    }
                }

                return members ?? new List<TeamMember>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.GetTeamMembersAsync error: {ex.Message}");
                return new List<TeamMember>();
            }
        }

        public async Task<TeamMember?> GetTeamMemberByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<TeamMember>($"api/members/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.GetTeamMemberByIdAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task AddTeamMemberAsync(TeamMember member)
        {
            try
            {
                await _http.PostAsJsonAsync("api/members", member);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.AddTeamMemberAsync error: {ex.Message}");
            }
        }

        public async Task UpdateTeamMemberAsync(TeamMember member)
        {
            try
            {
                await _http.PutAsJsonAsync($"api/members/{member.Id}", member);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.UpdateTeamMemberAsync error: {ex.Message}");
            }
        }

        public async Task DeleteTeamMemberAsync(int id)
        {
            try
            {
                await _http.DeleteAsync($"api/members/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.DeleteTeamMemberAsync error: {ex.Message}");
            }
        }

        public async Task SaveMetricRecordsForPeriodAsync(int memberId, string period, List<MetricRecord> records)
        {
            try
            {
                // Convert to API-compatible format (without navigation property)
                var apiRecords = records.Select(r => new 
                {
                    TeamMemberId = memberId,
                    Period = period,
                    MetricKey = r.MetricKey,
                    Value = r.Value
                }).ToList();

                await _http.PostAsJsonAsync($"api/metrics/{memberId}/{Uri.EscapeDataString(period)}", apiRecords);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.SaveMetricRecordsForPeriodAsync error: {ex.Message}");
            }
        }

        public async Task<List<MetricRecord>> GetMetricRecordsAsync(int? memberId = null, string? period = null)
        {
            try
            {
                var url = "api/metrics";
                var queryParams = new List<string>();
                
                if (memberId.HasValue)
                    queryParams.Add($"memberId={memberId.Value}");
                if (!string.IsNullOrWhiteSpace(period))
                    queryParams.Add($"period={Uri.EscapeDataString(period)}");
                
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var records = await _http.GetFromJsonAsync<List<MetricRecord>>(url);
                return records ?? new List<MetricRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.GetMetricRecordsAsync error: {ex.Message}");
                return new List<MetricRecord>();
            }
        }

        // Helper to get the latest period from a list of period strings
        private string? GetLatestPeriod(IEnumerable<string?> periodStrings)
        {
            return periodStrings
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => new { Period = p, EndDate = GetPeriodEndDate(p!) })
                .OrderByDescending(x => x.EndDate)
                .FirstOrDefault()?.Period;
        }

        private DateTime GetPeriodEndDate(string period)
        {
            try
            {
                string[] parts = period.Split('-');
                if (parts.Length < 2) return DateTime.MinValue;
                string monthYear = parts[1];
                DateTime monthStartDate = DateTime.ParseExact("01-" + monthYear, "dd-MMM yyyy", 
                    System.Globalization.CultureInfo.InvariantCulture);

                if (parts[0].Equals("Mid", StringComparison.OrdinalIgnoreCase))
                {
                    return monthStartDate.AddDays(14);
                }
                else // EOM
                {
                    return monthStartDate.AddMonths(1).AddDays(-1);
                }
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public async Task SavePodSnapshotAsync(GameScoreboard.Models.PodSnapshot snapshot)
        {
            try
            {
                await _http.PostAsJsonAsync("api/snapshot", snapshot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.SavePodSnapshotAsync error: {ex.Message}");
            }
        }

        public async Task<List<GameScoreboard.Models.PodSnapshot>> GetPodSnapshotsAsync(string? podName = null)
        {
            try
            {
                var url = "api/snapshot";
                if (!string.IsNullOrWhiteSpace(podName))
                    url += $"?podName={Uri.EscapeDataString(podName)}";
                var snapshots = await _http.GetFromJsonAsync<List<GameScoreboard.Models.PodSnapshot>>(url);
                return snapshots ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.GetPodSnapshotsAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<GameScoreboard.Models.PodSnapshot?> GetPodSnapshotByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<GameScoreboard.Models.PodSnapshot>($"api/snapshot/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpDataService.GetPodSnapshotByIdAsync error: {ex.Message}");
                return null;
            }
        }
    }
}

