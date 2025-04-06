using System;
using System.Collections.Generic;
using System.Linq;

namespace GameScoreboard.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = "Unknown";
        public string AvatarUrl { get; set; } = string.Empty;
        public Dictionary<string, object?> Metrics { get; set; } = new Dictionary<string, object?>();

        // Dictionary to map internal metric keys to their user-friendly display names
        private static readonly Dictionary<string, string> MetricDisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Computers
            { "M365Attach", "M365 Attach %" },
            { "GSP", "GSP %" }, // Generic GSP, context will clarify (Warranty, BP)
            { "Revenue", "Revenue $" }, // Generic Revenue, context will clarify (Comp Revenue, Store Revenue, Pick $)
            { "ASP", "ASP $" },
            { "Basket", "Basket $" }, // Generic Basket, context will clarify (Comp Basket, Store Basket)
            { "PMAttach", "PM Attach %" },
            // Store
            { "5Star", "5 Star Rating" }, // Generic 5-star
            // Front End
            { "BP", "BP #" }, // Branded Payments (Integer)
            { "PM", "PM #" }, // Paid Memberships (Integer)
            // Warehouse
            { "PickRate", "Pick Rate %" },
            { "PickQuantity", "Pick Quantity #" },
            { "Awk", "AWK (Mins)" }, // Awkward items time
            { "MVP", "MVP" }, // Selected MVP (string)
            { "Picks", "Total Picks" },
            { "Accuracy", "Pick Accuracy %" },
            { "Units", "Units Picked" }
        };

        // Dictionary to map metric keys to potential character titles when excelling
        private static readonly Dictionary<string, string> MetricTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
             // Computers
            { "M365Attach", "Microsoft Maestro" },
            { "GSP", "Guardian of Guarantees" }, // Generic title, could be adapted
            { "Revenue", "Revenue Raider" }, // Generic title
            { "ASP", "Price Paragon" },
            { "Basket", "Basket Baron" }, // Generic title
            { "PMAttach", "Membership Maximizer" },
             // Store
            { "5Star", "Customer Champion" }, // Generic title
             // Front End
            { "BP", "Payment Pro" },
            { "PM", "Membership Master" },
             // Warehouse
            { "PickRate", "Picking Prodigy" },
            { "PickQuantity", "Quantity King/Queen" },
            { "Awk", "Logistics Legend" },
            { "Pick$", "Value Voyager" }, // Added for Pick$ which is likely warehouse Revenue
            // MVP doesn't have a typical "best score" title, handled differently
        };

        // --- New Leveling System Properties ---
        public double TotalExperience { get; set; } = 0; // Persisted total accumulated XP

        public int CurrentLevel => (int)(TotalExperience / 100) + 1; // Level starts at 1
        
        public double ExperienceTowardsNextLevel => TotalExperience % 100;
        
        public double ExperienceNeededForNextLevel => 100;

        public double ProgressToNextLevelPercentage => (ExperienceTowardsNextLevel / ExperienceNeededForNextLevel) * 100;
        // -------------------------------------

        // Helper to safely get metric value as double?
        public double? GetMetricDoubleValue(string metricKey)
        {
            if (Metrics.TryGetValue(metricKey, out var value))
            {
                if (value is double d) return d;
                if (value is int i) return (double)i;
                if (value is decimal dec) return (double)dec;
                if (value is long l) return (double)l;
                // Could add more numeric types or try parsing if stored as string
            }
            return null; // Return null if key not found or value is not convertible to double
        }

        // Helper to get metric value as object?
        public object? GetMetricValue(string metricKey)
        {
            return Metrics.TryGetValue(metricKey, out var value) ? value : null;
        }

        // Get the strongest *numeric* metric relative to team averages
        public string GetStrongestMetricRelativeToTeam(Dictionary<string, double> teamAverages)
        {
            if (Metrics == null || !Metrics.Any() || teamAverages == null || !teamAverages.Any())
                return string.Empty; // Or a default metric key if appropriate

            var relativeStrengths = new Dictionary<string, double>();

            foreach (var metric in Metrics)
            {
                double? metricValue = GetMetricDoubleValue(metric.Key);
                if (metricValue.HasValue) // Only consider numeric metrics
                {
                    if (teamAverages.TryGetValue(metric.Key, out var avgValue) && avgValue != 0)
                    {
                        // Calculate relative strength (handle potential division by zero)
                        relativeStrengths[metric.Key] = (metricValue.Value / avgValue) * 100;
                    }
                    else
                    {
                        // If no average or average is 0, use the value itself (or handle differently)
                        relativeStrengths[metric.Key] = metricValue.Value;
                    }
                }
            }

            if (!relativeStrengths.Any())
                return string.Empty; // No numeric metrics found to compare

            // Return the metric key with the highest relative strength
            return relativeStrengths.OrderByDescending(rs => rs.Value).First().Key;
        }

        // Get the display name for a metric
        public static string GetMetricDisplayName(string metricKey)
        {
            // Handle specific cases like Pick$ using the generic Revenue display name
             if (metricKey == "Pick$") return "Pick Revenue $";

            return MetricDisplayNames.TryGetValue(metricKey, out var displayName)
                ? displayName
                : metricKey; // Return the key itself if no display name is found
        }

        // Get the title based on the metric where this member is team champion (numeric metrics only for score comparison)
        public string GetChampionTitle(List<TeamMember> allMembers)
        {
            if (allMembers == null || !allMembers.Any() || Metrics == null || !Metrics.Any())
                return "Team Member";

            var relevantMembers = allMembers.Where(m => m.Department == this.Department).ToList();
            if (!relevantMembers.Any()) return "Team Member";

            string? bestOverallMetric = null;
            double highestScore = double.MinValue;

            foreach (var metricKey in Metrics.Keys)
            {
                 // Only consider metrics that have titles defined (usually performance-based)
                if (!MetricTitles.ContainsKey(metricKey)) continue;

                double? memberScore = GetMetricDoubleValue(metricKey);
                if (!memberScore.HasValue) continue; // Skip non-numeric metrics

                // Find the best score for this numeric metric among relevant team members
                double? bestScoreNullable = relevantMembers
                                            .Select(m => m.GetMetricDoubleValue(metricKey))
                                            .Where(v => v.HasValue)
                                            .DefaultIfEmpty(null) // Handle case where no one has the metric
                                            .Max();

                if (!bestScoreNullable.HasValue) continue; // Skip if no one has this metric

                double bestScore = bestScoreNullable.Value;

                // If this member has the best score (allowing for small tolerance)
                 // Consider a small tolerance for floating point comparisons if necessary
                 // Example: >= bestScore * 0.999
                if (memberScore.Value >= bestScore)
                {
                    // If this score is higher than the current highest score found across all metrics
                    if (memberScore.Value > highestScore)
                    {
                         highestScore = memberScore.Value;
                         bestOverallMetric = metricKey;
                    }
                    // If scores are equal, maybe prioritize based on a predefined list or keep the first one found
                }
            }

             // If a best overall metric was found where this member excels
             if (bestOverallMetric != null && MetricTitles.TryGetValue(bestOverallMetric, out var title))
             {
                return title;
             }

            // Fallback title if not a champion in any specific metric
            return "Team Contributor"; // Or another suitable default
        }

        // Determine if this member is the best in a specific *numeric* metric
        public bool IsBestInMetric(string metricKey, List<TeamMember> allMembers)
        {
            if (allMembers == null || !allMembers.Any() || !Metrics.ContainsKey(metricKey))
                return false;

            var relevantMembers = allMembers.Where(m => m.Department == this.Department).ToList();
             if (!relevantMembers.Any()) return false;

            double? memberScore = GetMetricDoubleValue(metricKey);
            if (!memberScore.HasValue) return false; // Cannot be best if not numeric

            double? bestScore = relevantMembers
                                .Select(m => m.GetMetricDoubleValue(metricKey))
                                .Where(v => v.HasValue)
                                .DefaultIfEmpty(null)
                                .Max();

            // Check if member's score is the best (consider tolerance if needed)
            return bestScore.HasValue && memberScore.Value >= bestScore.Value;
        }

        // Determine if this member is the worst in a specific *numeric* metric
        public bool IsWorstInMetric(string metricKey, List<TeamMember> allMembers)
        {
             if (allMembers == null || !allMembers.Any() || !Metrics.ContainsKey(metricKey))
                return false;

            var relevantMembers = allMembers.Where(m => m.Department == this.Department).ToList();
             if (!relevantMembers.Any()) return false;

            double? memberScore = GetMetricDoubleValue(metricKey);
            if (!memberScore.HasValue) return false; // Cannot be worst if not numeric

            double? worstScore = relevantMembers
                                 .Select(m => m.GetMetricDoubleValue(metricKey))
                                 .Where(v => v.HasValue)
                                 .DefaultIfEmpty(null)
                                 .Min();

            // Check if member's score is the worst (consider tolerance if needed)
             return worstScore.HasValue && memberScore.Value <= worstScore.Value;
        }

        // Get all available metrics as key-value pairs
        public Dictionary<string, object?> GetAllMetrics()
        {
            return new Dictionary<string, object?>(Metrics);
        }
    }
}