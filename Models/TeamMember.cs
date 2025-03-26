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
            { "M365Attach", "Microsoft Champion" },
            { "GSP", "Guardian of Guarantees" },
            { "Revenue", "Revenue Raider" },
            { "ASP", "Elite Virtuoso" },
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