using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameScoreboard.Models;

namespace GameScoreboard.Services
{
    public class AppState
    {
        public event Func<Task>? OnMetricsUpdatedAsync;

        public List<TeamMember>? MemberCache { get; private set; }
        public DateTime? CacheTimestamp { get; private set; }
        private Task? _preloadTask;

        public async Task NotifyMetricsUpdatedAsync()
        {
            InvalidateCache();
            if (OnMetricsUpdatedAsync != null)
            {
                await OnMetricsUpdatedAsync.Invoke();
            }
        }

        public void InvalidateCache()
        {
            MemberCache = null;
            CacheTimestamp = null;
            _preloadTask = null;
        }

        public void StartPreload(IDataService dataService)
        {
            if (_preloadTask != null || MemberCache != null) return;
            _preloadTask = Task.Run(async () =>
            {
                try
                {
                    var members = await dataService.GetTeamMembersAsync();
                    MemberCache = members;
                    CacheTimestamp = DateTime.UtcNow;
                    Console.WriteLine($"[AppState] Preloaded {members?.Count ?? 0} members into cache.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AppState] Preload failed: {ex.Message}");
                }
            });
        }

        public async Task<List<TeamMember>> GetOrFetchMembersAsync(IDataService dataService, string? department = null)
        {
            if (_preloadTask != null && MemberCache == null)
            {
                await _preloadTask;
            }

            if (MemberCache != null)
            {
                if (string.IsNullOrEmpty(department))
                    return MemberCache;
                return MemberCache.FindAll(m => m.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
            }

            var members = await dataService.GetTeamMembersAsync(department);
            if (string.IsNullOrEmpty(department))
            {
                MemberCache = members;
                CacheTimestamp = DateTime.UtcNow;
            }
            return members ?? new();
        }
    }
}