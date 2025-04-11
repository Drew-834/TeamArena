using System.Collections.Generic;
using System.Linq;
using GameScoreboard.Models;

namespace GameScoreboard.Models
{
    public class DepartmentSummary
    {
        public Dictionary<string, double> AverageMetrics { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> MaxMetrics { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> MinMetrics { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, string> TopPerformers { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> BottomPerformers { get; set; } = new Dictionary<string, string>();
        public int TotalTeamMembers { get; set; }

        public static DepartmentSummary CalculateSummary(List<TeamMember> teamMembers)
        {
            if (teamMembers == null || !teamMembers.Any())
                return new DepartmentSummary();

            var summary = new DepartmentSummary
            {
                TotalTeamMembers = teamMembers.Count
            };

            // Dynamically get all unique metric keys present across all team members
            var allMetricKeys = teamMembers.SelectMany(m => m.MetricRecords.Select(mr => mr.MetricKey)).Distinct().ToList();

            // Calculate averages, max, min, and performers for each *numeric* metric
            foreach (var key in allMetricKeys)
            {
                // Get all non-null double values for this metric key across the team
                var values = teamMembers
                    .Select(m => m.GetMetricDoubleValue(key))
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value) // Use null-forgiving operator (!)
                    .ToList();

                // Only proceed if there are numeric values for this key
                if (values.Any())
                {
                    // Calculate Average, Max, Min
                    summary.AverageMetrics[key] = values.Average();
                    summary.MaxMetrics[key] = values.Max();
                    summary.MinMetrics[key] = values.Min();

                    // Find top performer (handle potential ties - FirstOrDefault used here)
                    var maxValue = summary.MaxMetrics[key];
                    var topPerformer = teamMembers.FirstOrDefault(m => (m.GetMetricDoubleValue(key) ?? 0) >= maxValue);
                    if (topPerformer != null)
                    {
                        summary.TopPerformers[key] = topPerformer.Name;
                    }

                    // Find bottom performer (handle potential ties - FirstOrDefault used here)
                    var minValue = summary.MinMetrics[key];
                    var bottomPerformer = teamMembers.FirstOrDefault(m => (m.GetMetricDoubleValue(key) ?? 0) <= minValue);
                    if (bottomPerformer != null)
                    {
                        summary.BottomPerformers[key] = bottomPerformer.Name;
                    }
                }
                // Non-numeric metrics (like MVP) are implicitly skipped by this logic
            }

            return summary;
        }
    }
}