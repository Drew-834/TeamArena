using GameScoreboard.Models;
using GameScoreboard.Data;
using System.Net.Http.Json;

namespace GameScoreboard.Services
{
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
                    url += $"?department={Uri.EscapeDataString(department)}";

                Console.WriteLine($"[HttpDataService] GET {url}");
                var sw = System.Diagnostics.Stopwatch.StartNew();

                var members = await _http.GetFromJsonAsync<List<TeamMember>>(url);
                sw.Stop();

                if (members == null || !members.Any())
                {
                    Console.WriteLine($"[HttpDataService] GET {url} returned 0 members in {sw.ElapsedMilliseconds}ms");
                    return new List<TeamMember>();
                }

                var withMetrics = members.Count(m => m.MetricRecords?.Any() == true);
                Console.WriteLine($"[HttpDataService] GET {url} returned {members.Count} members ({withMetrics} with metrics) in {sw.ElapsedMilliseconds}ms");

                return members;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] GetTeamMembersAsync FAILED: {ex.Message}");
                return new List<TeamMember>();
            }
        }

        public async Task<TeamMember?> GetTeamMemberByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"[HttpDataService] GET api/members/{id}");
                return await _http.GetFromJsonAsync<TeamMember>($"api/members/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] GetTeamMemberByIdAsync({id}) FAILED: {ex.Message}");
                return null;
            }
        }

        public async Task AddTeamMemberAsync(TeamMember member)
        {
            try
            {
                Console.WriteLine($"[HttpDataService] POST api/members: {member.Name} ({member.Department})");
                var response = await _http.PostAsJsonAsync("api/members", member);

                if (response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<TeamMember>();
                    if (created != null)
                    {
                        member.Id = created.Id;
                        Console.WriteLine($"[HttpDataService] Created member '{member.Name}' with Id={member.Id}");
                    }
                }
                else
                {
                    Console.WriteLine($"[HttpDataService] AddTeamMember FAILED: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] AddTeamMemberAsync FAILED: {ex.Message}");
            }
        }

        public async Task UpdateTeamMemberAsync(TeamMember member)
        {
            try
            {
                Console.WriteLine($"[HttpDataService] PUT api/members/{member.Id}: {member.Name}");
                var payload = new
                {
                    member.Id,
                    member.Name,
                    member.Department,
                    member.Role,
                    member.AvatarUrl,
                    member.TotalExperience,
                    member.RphGoal,
                    member.AppEffGoal,
                    member.PmEffGoal,
                    member.WarrantyAttachGoal,
                    member.AccAttachGoal
                };
                var response = await _http.PutAsJsonAsync($"api/members/{member.Id}", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[HttpDataService] UpdateTeamMember FAILED: {response.StatusCode} - {body}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] UpdateTeamMemberAsync FAILED: {ex.Message}");
            }
        }

        public async Task DeleteTeamMemberAsync(int id)
        {
            try
            {
                Console.WriteLine($"[HttpDataService] DELETE api/members/{id}");
                await _http.DeleteAsync($"api/members/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] DeleteTeamMemberAsync FAILED: {ex.Message}");
            }
        }

        public async Task SaveMetricRecordsForPeriodAsync(int memberId, string period, List<MetricRecord> records)
        {
            try
            {
                var apiRecords = records.Select(r => new
                {
                    TeamMemberId = memberId,
                    Period = period,
                    MetricKey = r.MetricKey,
                    Value = r.Value
                }).ToList();

                var url = $"api/metrics/{memberId}/{Uri.EscapeDataString(period)}";
                Console.WriteLine($"[HttpDataService] POST {url}: {apiRecords.Count} records");

                var response = await _http.PostAsJsonAsync(url, apiRecords);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[HttpDataService] SaveMetrics FAILED for member {memberId}: {response.StatusCode} - {body}");
                }
                else
                    Console.WriteLine($"[HttpDataService] SaveMetrics OK for member {memberId}, period={period}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] SaveMetricRecordsForPeriodAsync FAILED: {ex.Message}");
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

                Console.WriteLine($"[HttpDataService] GET {url}");
                var records = await _http.GetFromJsonAsync<List<MetricRecord>>(url);
                Console.WriteLine($"[HttpDataService] GET {url} returned {records?.Count ?? 0} records");
                return records ?? new List<MetricRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] GetMetricRecordsAsync FAILED: {ex.Message}");
                return new List<MetricRecord>();
            }
        }

        public async Task SavePodSnapshotAsync(GameScoreboard.Models.PodSnapshot snapshot)
        {
            try
            {
                Console.WriteLine($"[HttpDataService] POST api/snapshot: {snapshot.PodName}");
                await _http.PostAsJsonAsync("api/snapshot", snapshot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDataService] SavePodSnapshotAsync FAILED: {ex.Message}");
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
                Console.WriteLine($"[HttpDataService] GetPodSnapshotsAsync FAILED: {ex.Message}");
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
                Console.WriteLine($"[HttpDataService] GetPodSnapshotByIdAsync FAILED: {ex.Message}");
                return null;
            }
        }
    }
}
