using System.Collections.Generic;
using System.Linq;

namespace GameScoreboard.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public Dictionary<string, double> Metrics { get; set; }

        // Dictionary to map metric names to their display names
        private static readonly Dictionary<string, string> MetricDisplayNames = new()
        {
            { "M365Attach", "M365 Attach" },
            { "GSP", "GSP (Warranty)" },
            { "Revenue", "Revenue" },
            { "ASP", "ASP (Average Selling Price)" },
            { "Basket", "Basket" },
            { "PMAttach", "PM Attach %" }
        };

        // Dictionary to map metric names to character titles
        private static readonly Dictionary<string, string> MetricTitles = new()
      {
            { "M365Attach   ", "Microsoft Champion" },
            { "GSP", "Guardian of Guarantees" },
            { "Revenue", "Revenue Raider" },
            { "ASP", "Highest Bidder" },
            { "Basket", "Master of Addons" },
            { "PMAttach", "Membership Pro" }
        };

        // Get the strongest metric for this team member
        public string GetStrongestMetric()
        {
            if (Metrics == null || !Metrics.Any())
                return string.Empty;

            return Metrics.OrderByDescending(m => m.Value).First().Key;
        }

        // Get the strongest metric relative to team averages
        public string GetStrongestMetricRelativeToTeam(Dictionary<string, double> teamAverages)
        {
            if (Metrics == null || !Metrics.Any() || teamAverages == null || !teamAverages.Any())
                return GetStrongestMetric();

            var relativeStrengths = new Dictionary<string, double>();

            foreach (var metric in Metrics)
            {
                if (teamAverages.TryGetValue(metric.Key, out var avgValue) && avgValue > 0)
                {
                    // Calculate how much better than average they are (as a percentage)
                    relativeStrengths[metric.Key] = (metric.Value / avgValue) * 100;
                }
                else
                {
                    relativeStrengths[metric.Key] = metric.Value;
                }
            }

            // Return the metric where they're strongest relative to team average
            return relativeStrengths.OrderByDescending(m => m.Value).First().Key;
        }

        // Get the display name for a metric
        public static string GetMetricDisplayName(string metricKey)
        {
            return MetricDisplayNames.TryGetValue(metricKey, out var displayName)
                ? displayName
                : metricKey;
        }

        // Get the title based on the strongest metric
        public string GetTitle()
        {
            var strongestMetric = GetStrongestMetric();
            return MetricTitles.TryGetValue(strongestMetric, out var title)
                ? title
                : "Team Member";
        }

        // Get the title based on the metric where this member is team champion
        public string GetChampionTitle(List<TeamMember> allMembers)
        {
            if (allMembers == null || !allMembers.Any() || Metrics == null || !Metrics.Any())
                return "Team Member";

            // Get all metrics where this member could be a champion
            var championMetrics = new List<string>();

            foreach (var metric in Metrics)
            {
                // Find the best score for this metric among all team members
                var bestScore = allMembers.Max(m => m.GetMetricValue(metric.Key));

                // If this member is within 3 units of the best (or is the best), add to champion metrics
                if (bestScore - metric.Value <= 3 || metric.Value >= bestScore * 0.97)
                {
                    championMetrics.Add(metric.Key);
                }
            }

            // If no champion metrics found, return based on strongest personal metric
            if (!championMetrics.Any())
                return GetTitle();

            // First, check if they are the absolute best in any metric
            foreach (var metric in championMetrics)
            {
                var bestScore = allMembers.Max(m => m.GetMetricValue(metric));
                if (GetMetricValue(metric) >= bestScore * 0.99) // Essentially equal to the best
                {
                    return MetricTitles.TryGetValue(metric, out var title) ? title : "Team Champion";
                }
            }

            // If not the absolute best, then use their strongest champion metric
            var strongestChampionMetric = championMetrics
                .OrderByDescending(m => GetMetricValue(m))
                .First();

            return MetricTitles.TryGetValue(strongestChampionMetric, out var championTitle)
                ? championTitle
                : "Team Champion";
        }

        // Determine if this member is the best in a specific metric
        public bool IsBestInMetric(string metricKey, List<TeamMember> allMembers)
        {
            if (allMembers == null || !allMembers.Any() || !Metrics.ContainsKey(metricKey))
                return false;

            var bestScore = allMembers.Max(m => m.GetMetricValue(metricKey));
            return GetMetricValue(metricKey) >= bestScore * 0.99; // Within 1% of the best
        }

        // Determine if this member is the worst in a specific metric
        public bool IsWorstInMetric(string metricKey, List<TeamMember> allMembers)
        {
            if (allMembers == null || !allMembers.Any() || !Metrics.ContainsKey(metricKey))
                return false;

            var worstScore = allMembers.Min(m => m.GetMetricValue(metricKey));
            return GetMetricValue(metricKey) <= worstScore * 1.01; // Within 1% of the worst
        }

        // Get the value of a specific metric
        public double GetMetricValue(string metricKey)
        {
            return Metrics.TryGetValue(metricKey, out var value) ? value : 0;
        }

        // Get all available metrics as a list
        public IEnumerable<KeyValuePair<string, double>> GetAllMetrics()
        {
            return Metrics ?? new Dictionary<string, double>();
        }
    }
}