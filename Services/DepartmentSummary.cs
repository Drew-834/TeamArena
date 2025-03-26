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
        public double OverallPerformanceScore { get; set; }

        public static DepartmentSummary CalculateSummary(List<TeamMember> teamMembers)
        {
            if (teamMembers == null || !teamMembers.Any())
                return new DepartmentSummary();

            var summary = new DepartmentSummary
            {
                TotalTeamMembers = teamMembers.Count
            };

            // Get all metric keys from the first team member
            var metricKeys = teamMembers.First().Metrics.Keys.ToList();

            // Calculate averages, max, min and top performers for each metric
            foreach (var key in metricKeys)
            {
                // Get all values for this metric
                var values = teamMembers.Select(m => m.GetMetricValue(key)).ToList();

                // Calculate average
                summary.AverageMetrics[key] = values.Average();

                // Calculate max
                summary.MaxMetrics[key] = values.Max();

                // Calculate min
                summary.MinMetrics[key] = values.Min();

                // Find top performer for this metric
                var maxValue = summary.MaxMetrics[key];
                var topPerformer = teamMembers.FirstOrDefault(m => m.GetMetricValue(key) >= maxValue * 0.98);
                if (topPerformer != null)
                {
                    summary.TopPerformers[key] = topPerformer.Name;
                }

                // Find bottom performer for this metric
                var minValue = summary.MinMetrics[key];
                var bottomPerformer = teamMembers.FirstOrDefault(m => m.GetMetricValue(key) <= minValue * 1.02);
                if (bottomPerformer != null)
                {
                    summary.BottomPerformers[key] = bottomPerformer.Name;
                }
            }

            // Calculate overall performance score (average of all normalized metrics)
            var overallScores = new List<double>();
            foreach (var member in teamMembers)
            {
                double memberScore = 0;
                foreach (var key in metricKeys)
                {
                    // Normalize metric value against max possible value
                    var normalizedValue = NormalizeMetricValue(key, member.GetMetricValue(key));
                    memberScore += normalizedValue;
                }
                overallScores.Add(memberScore / metricKeys.Count);
            }

            summary.OverallPerformanceScore = overallScores.Average() * 100;

            return summary;
        }

        private static double NormalizeMetricValue(string metricKey, double value)
        {
            // Maximum reference values for each metric
            var maxValues = new Dictionary<string, double>
            {
               { "M365Attach", 30 },
               { "GSP", 20 },
               { "Revenue", 20000 },
               { "ASP", 800 },
               { "Basket", 160 },
               { "PMAttach", 25 }
            };

            double maxValue = maxValues.ContainsKey(metricKey) ? maxValues[metricKey] : 100;
            return System.Math.Min(value / maxValue, 1);
        }
    }
}