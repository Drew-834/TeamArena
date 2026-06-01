using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GameScoreboard.Data;
using GameScoreboard.Models;

namespace GameScoreboard.Services.Tracking;

public static class PodLeaderboardCalculator
{
    public static string? DetermineLatestPeriod(IEnumerable<TeamMember> members)
    {
        return members
            .SelectMany(m => m.MetricRecords ?? Enumerable.Empty<MetricRecord>())
            .Select(r => r.Period)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(p => new { Period = p, EndDate = PeriodHelper.GetPeriodEndDate(p) })
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefault()
            ?.Period;
    }

    public static List<RankedMember> CalculatePodLeaderboard(IEnumerable<TeamMember> members, string? period)
    {
        var podGroups = members
            .Where(m => PodMetricCatalog.IsPodDepartment(m.Department))
            .GroupBy(m => m.Department, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return podGroups
            .Select(group => new RankedMember
            {
                Name = group.Key,
                Department = group.Key,
                Score = group
                    .Select(member => GetWeightedScore(member, period))
                    .Where(score => score > 0)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .Where(rank => rank.Score > 0)
            .OrderByDescending(rank => rank.Score)
            .Select((rank, index) =>
            {
                rank.Id = index;
                rank.Rank = index + 1;
                return rank;
            })
            .ToList();
    }

    public static double GetWeightedScore(TeamMember member, string? period)
    {
        double totalWeightedScore = 0;
        int applicableWeight = 0;

        foreach (var metricKey in PodMetricCatalog.GetApplicableMetrics(member.Department))
        {
            int weight = PodMetricCatalog.GetWeight(metricKey);
            double? rawValue = GetMetricValue(member, metricKey, period);
            if (!rawValue.HasValue)
            {
                continue;
            }

            if ((metricKey == "AppEff" || metricKey == "PMEff") && rawValue.Value >= 20000)
            {
                continue;
            }

            if (metricKey == "Surveys" && rawValue.Value == 0)
            {
                applicableWeight += weight;
                continue;
            }

            double target = PodMetricCatalog.GetTarget(metricKey);
            double normalizedScore = PodMetricCatalog.IsLowerBetter(metricKey)
                ? (target > 0 && rawValue.Value > 0 ? (target / rawValue.Value) * 100.0 : 0)
                : (target > 0 ? (rawValue.Value / target) * 100.0 : 0);

            if (normalizedScore > 100)
            {
                double excess = normalizedScore - 100;
                double bonus = 15.0 * Math.Log(1 + excess / 20.0);
                normalizedScore = 100 + Math.Min(bonus, 25.0);
            }

            normalizedScore = Math.Clamp(normalizedScore, 0, 125);
            totalWeightedScore += normalizedScore * weight;
            applicableWeight += weight;
        }

        return applicableWeight > 0 ? Math.Min(totalWeightedScore / applicableWeight, 125) : 0;
    }

    public static double? GetMetricValue(TeamMember member, string metricKey, string? period)
    {
        foreach (var lookupKey in PodMetricCatalog.GetLookupKeys(metricKey, member.Department))
        {
            var matchingRecords = member.MetricRecords?
                .Where(r => r.MetricKey.Equals(lookupKey, StringComparison.OrdinalIgnoreCase));

            MetricRecord? record = string.IsNullOrWhiteSpace(period)
                ? matchingRecords?
                    .OrderByDescending(r => PeriodHelper.GetPeriodEndDate(r.Period))
                    .FirstOrDefault()
                : matchingRecords?
                    .FirstOrDefault(r => r.Period == period);

            if (record?.Value != null &&
                double.TryParse(record.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }
        }

        return null;
    }

}
