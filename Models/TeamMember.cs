using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameScoreboard.Data; // Added for MetricRecord

namespace GameScoreboard.Models
{
    public class TeamMember
    {
        [Key] // Ensure EF Core recognizes this as the primary key
        public int Id { get; set; }
        
        [Required]
        public required string Name { get; set; }
        
        [Required]
        public required string Department { get; set; }
        
        public string? Role { get; set; } // Optional role/title
        
        public string? AvatarUrl { get; set; } // Optional URL for profile picture
        
        public double TotalExperience { get; set; } = 0; // Initialize XP to 0

        // Navigation property for related metric records
        public ICollection<MetricRecord> MetricRecords { get; set; } = new List<MetricRecord>();

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
        public int CurrentLevel => (int)(TotalExperience / 100) + 1; // Level starts at 1
        
        public double ExperienceTowardsNextLevel => TotalExperience % 100;
        
        public double ExperienceNeededForNextLevel => 100;

        public double ProgressToNextLevelPercentage => (ExperienceTowardsNextLevel / ExperienceNeededForNextLevel) * 100;
        // -------------------------------------

        // Helper to safely get metric value as double?
        public double? GetMetricDoubleValue(string key)
        {
            // Placeholder - Needs refactoring
            var record = MetricRecords.FirstOrDefault(mr => mr.MetricKey.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (record?.Value != null && double.TryParse(record.Value, out double val))
            {
                return val;
            }
            return null; 
        }

        public string GetMetricStringValue(string key)
        { 
            // Placeholder - Needs refactoring
             var record = MetricRecords.FirstOrDefault(mr => mr.MetricKey.Equals(key, StringComparison.OrdinalIgnoreCase));
            return record?.Value ?? "N/A";
        }

        public object? GetMetricValue(string key)
        {
            // Placeholder - Needs refactoring
             var record = MetricRecords.FirstOrDefault(mr => mr.MetricKey.Equals(key, StringComparison.OrdinalIgnoreCase));
             // Attempt to parse, but might just return string for simplicity now
             if (record?.Value != null)
             {
                 if (double.TryParse(record.Value, out double dVal)) return dVal;
                 if (int.TryParse(record.Value, out int iVal)) return iVal;
                 return record.Value; // Return as string if not numeric
             }
            return null;
        }

        // Get the strongest *numeric* metric relative to team averages
        public string GetStrongestMetricRelativeToTeam(Dictionary<string, double> teamAverages)
        {
            if (MetricRecords == null || !MetricRecords.Any() || teamAverages == null || !teamAverages.Any())
                return string.Empty; // Or a default metric key if appropriate

            var relativeStrengths = new Dictionary<string, double>();

            foreach (var record in MetricRecords)
            {
                double? metricValue = GetMetricDoubleValue(record.MetricKey);
                if (metricValue.HasValue) // Only consider numeric metrics
                {
                    if (teamAverages.TryGetValue(record.MetricKey, out var avgValue) && avgValue != 0)
                    {
                        // Calculate relative strength (handle potential division by zero)
                        relativeStrengths[record.MetricKey] = (metricValue.Value / avgValue) * 100;
                    }
                    else
                    {
                        // If no average or average is 0, use the value itself (or handle differently)
                        relativeStrengths[record.MetricKey] = metricValue.Value;
                    }
                }
            }

            if (!relativeStrengths.Any())
                return string.Empty; // No numeric metrics found to compare

            // Return the metric key with the highest relative strength
            return relativeStrengths.OrderByDescending(rs => rs.Value).First().Key;
        }

        // Get the display name for a metric
        public static string GetMetricDisplayName(string key)
        {
            // This helper can likely remain as is
             return key switch
            {
                "M365Attach" => "M365 Attach %",
                "GSP" => "GSP Attach %",
                "PMAttach" => "PM Attach %",
                "Revenue" => "Revenue",
                "ASP" => "ASP",
                "Basket" => "Avg Basket",
                "5Star" => "5-Star",
                "BP" => "BP Count",
                "PM" => "PM Count",
                "Picks" => "Picks Qty",
                "Accuracy" => "Pick Acc %",
                "Awk" => "Awkward Qty", 
                "Units" => "Units Picked",
                _ => key // Default to key if not found
            };
        }

        // Get the title based on the metric where this member is team champion (numeric metrics only for score comparison)
        public string GetChampionTitle(List<TeamMember> allMembers)
        {
            if (allMembers == null || !allMembers.Any() || MetricRecords == null || !MetricRecords.Any())
                return "Team Member";

            var relevantMembers = allMembers.Where(m => m.Department == this.Department).ToList();
            if (!relevantMembers.Any()) return "Team Member";

            string? bestOverallMetric = null;
            double highestScore = double.MinValue;

            foreach (var record in MetricRecords)
            {
                 // Only consider metrics that have titles defined (usually performance-based)
                if (!MetricTitles.ContainsKey(record.MetricKey)) continue;

                double? memberScore = GetMetricDoubleValue(record.MetricKey);
                if (!memberScore.HasValue) continue; // Skip non-numeric metrics

                // Find the best score for this numeric metric among relevant team members
                double? bestScoreNullable = relevantMembers
                                            .Select(m => m.GetMetricDoubleValue(record.MetricKey))
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
                         bestOverallMetric = record.MetricKey;
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
            // Check for null/empty list first to address CS8604 before LINQ
            if (allMembers == null || !allMembers.Any())
                return false;
            // Corrected check for metric key existence (Fixes CS1503)
            if (MetricRecords == null || !MetricRecords.Any(mr => mr.MetricKey.Equals(metricKey, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Now safe to use LINQ on allMembers
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
            // Check for null/empty list first to address CS8604 before LINQ
            if (allMembers == null || !allMembers.Any())
                 return false;
            // Corrected check for metric key existence (Fixes CS1503)
            if (MetricRecords == null || !MetricRecords.Any(mr => mr.MetricKey.Equals(metricKey, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Now safe to use LINQ on allMembers
            var relevantMembers = allMembers.Where(m => m.Department == this.Department).ToList();
            if (!relevantMembers.Any()) return false; // Should not happen if member is in allMembers, but safe check

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

        // Gets all metrics for the member as a dictionary
        public Dictionary<string, object?> GetAllMetrics()
        {
            // Corrected implementation to return correct type (Fixes CS0029)
            var metricsDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (MetricRecords != null)
            {
                foreach (var record in MetricRecords)
                {
                    // Attempt parsing directly from the record in the loop
                    object? displayValue = record.Value; // Default to string
                    if (record.Value != null)
                    {
                        if (double.TryParse(record.Value, out double dVal))
                        {
                            displayValue = dVal;
                        }
                        else if (int.TryParse(record.Value, out int iVal))
                        {
                             displayValue = iVal;
                        }
                    }
                    metricsDict[record.MetricKey] = displayValue; 
                }
            }
            return metricsDict;
        }
    }
}