using System.Net.Http.Json;
using GameScoreboard.Data;
using GameScoreboard.Models;

namespace GameScoreboard.Services
{
    public class HttpDataService : IDataService
    {
        private readonly HttpClient _httpClient;

        public HttpDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<List<TeamMember>> GetTeamMembersAsync(string? department = null)
        {
            var url = string.IsNullOrEmpty(department) ? "api/members" : $"api/members?department={Uri.EscapeDataString(department)}";
            return _httpClient.GetFromJsonAsync<List<TeamMember>>(url)!;
        }

        public Task<TeamMember?> GetTeamMemberByIdAsync(int id)
            => _httpClient.GetFromJsonAsync<TeamMember>($"api/members/{id}");

        public Task AddTeamMemberAsync(TeamMember member)
            => Task.CompletedTask; // Not used yet

        public async Task UpdateTeamMemberAsync(TeamMember member)
            => await _httpClient.PutAsJsonAsync($"api/members/{member.Id}", member);

        public Task DeleteTeamMemberAsync(int id)
            => Task.CompletedTask; // Not used yet

        public async Task SaveMetricRecordsForPeriodAsync(int memberId, string period, List<MetricRecord> records)
            => await _httpClient.PostAsJsonAsync($"api/metrics/{memberId}/{Uri.EscapeDataString(period)}", records);

        public Task<List<MetricRecord>> GetMetricRecordsAsync(int? memberId = null, string? period = null)
        {
            var qs = new List<string>();
            if (memberId.HasValue) qs.Add($"memberId={memberId.Value}");
            if (!string.IsNullOrEmpty(period)) qs.Add($"period={Uri.EscapeDataString(period)}");
            var url = "api/metrics" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
            return _httpClient.GetFromJsonAsync<List<MetricRecord>>(url)!;
        }
    }
} 