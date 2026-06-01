using GameScoreboard.Data;
using GameScoreboard.Models;

namespace GameScoreboard.Services.Tracking;

public static class PerformanceReviewBuilder
{
    public static PerformanceReviewResult? Build(
        TeamMember member,
        IReadOnlyList<MetricRecord> memberRecords,
        IReadOnlyDictionary<string, double> teamAverages,
        IReadOnlyList<TeamMember> podPeers)
    {
        if (!PodMetricCatalog.IsPodDepartment(member.Department))
            return null;

        var periods = memberRecords.Select(r => r.Period).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var currentPeriod = PeriodHelper.GetLatestPeriod(periods);
        if (currentPeriod == null)
            return null;

        var priorPeriod = PeriodHelper.GetPriorPeriod(periods, currentPeriod);

        member.MetricRecords = memberRecords.ToList();

        var slides = new List<KpiReviewSlide>();
        foreach (var metricKey in PodMetricCatalog.GetApplicableMetrics(member.Department)
                     .OrderByDescending(PodMetricCatalog.GetWeight))
        {
            var slide = BuildSlide(member, metricKey, currentPeriod, priorPeriod, teamAverages);
            if (slide != null)
                slides.Add(slide);
        }

        if (!slides.Any(s => s.Tier != PerformanceReviewTier.NoData))
            return null;

        double weightedScore = PodLeaderboardCalculator.GetWeightedScore(member, currentPeriod);
        double? priorWeightedScore = priorPeriod != null
            ? PodLeaderboardCalculator.GetWeightedScore(member, priorPeriod)
            : null;

        double? scoreDelta = priorWeightedScore.HasValue
            ? weightedScore - priorWeightedScore.Value
            : null;

        var strengths = slides
            .Where(s => s.Tier is PerformanceReviewTier.Exceptional or PerformanceReviewTier.Strong)
            .OrderByDescending(s => s.Score)
            .Take(2)
            .Select(s => s.DisplayName)
            .ToList();

        var focusAreas = slides
            .Where(s => s.Tier is PerformanceReviewTier.Concern or PerformanceReviewTier.Fair)
            .OrderByDescending(s => s.Weight * Math.Max(0, 100 - s.GoalProgressPercent))
            .Take(3)
            .Select(s => s.DisplayName)
            .ToList();

        var peersInPod = podPeers
            .Where(m => m.Department.Equals(member.Department, StringComparison.OrdinalIgnoreCase))
            .ToList();
        int podCount = peersInPod.Count;

        int? podRank = null;
        if (peersInPod.Count > 0)
        {
            var ranked = peersInPod
                .Select(m => new { m.Id, Score = PodLeaderboardCalculator.GetWeightedScore(m, currentPeriod) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ToList();

            int index = ranked.FindIndex(x => x.Id == member.Id);
            if (index >= 0)
                podRank = index + 1;
        }

        return new PerformanceReviewResult(
            member,
            currentPeriod,
            priorPeriod,
            slides,
            weightedScore,
            priorWeightedScore,
            scoreDelta,
            podRank,
            podCount,
            strengths,
            focusAreas);
    }

    public static Dictionary<string, double> ComputeTeamAverages(
        IReadOnlyList<MetricRecord> deptRecords,
        string period)
    {
        var averages = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var keys = deptRecords
            .Where(r => r.Period == period)
            .Select(r => r.MetricKey)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var key in keys)
        {
            var values = deptRecords
                .Where(r => r.Period == period
                    && r.MetricKey.Equals(key, StringComparison.OrdinalIgnoreCase)
                    && r.Value != null)
                .Select(r => TrackerValueSanitizer.ParseNullableDouble(r.Value))
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            if (PerformanceScorer.IsPodEfficiency(key))
                values = values.Where(v => v < 20000).ToList();

            if (key.Equals("Surveys", StringComparison.OrdinalIgnoreCase))
                values = values.Where(v => v > 0).ToList();

            if (values.Count > 0)
                averages[key] = values.Average();
        }

        return averages;
    }

    private static KpiReviewSlide? BuildSlide(
        TeamMember member,
        string metricKey,
        string currentPeriod,
        string? priorPeriod,
        IReadOnlyDictionary<string, double> teamAverages)
    {
        double? currentValue = PodLeaderboardCalculator.GetMetricValue(member, metricKey, currentPeriod);
        if (!currentValue.HasValue)
            return null;

        if (PodMetricCatalog.IsIgnoredNumericValue(metricKey, currentValue.Value))
            return null;

        double? priorValue = priorPeriod != null
            ? PodLeaderboardCalculator.GetMetricValue(member, metricKey, priorPeriod)
            : null;

        double goal = PerformanceScorer.GetMemberGoal(member, metricKey) ?? PodMetricCatalog.GetTarget(metricKey);
        double score = PerformanceScorer.CalculatePerformanceScore(metricKey, currentValue.Value, teamAverages, member);
        double? priorScore = priorValue.HasValue
            ? PerformanceScorer.CalculatePerformanceScore(metricKey, priorValue.Value, teamAverages, member)
            : null;

        var (deltaAbs, deltaPct) = ComputeDelta(metricKey, currentValue.Value, priorValue);

        return new KpiReviewSlide(
            metricKey,
            TeamMember.GetMetricDisplayName(metricKey),
            PodMetricCatalog.GetWeight(metricKey),
            currentValue,
            priorValue,
            goal,
            score,
            priorScore,
            PerformanceScorer.GetGoalProgressPercent(metricKey, currentValue.Value, goal),
            deltaAbs,
            deltaPct,
            PerformanceScorer.GetTier(score, true));
    }

    private static (double? Absolute, double? Percent) ComputeDelta(
        string metricKey,
        double current,
        double? prior)
    {
        if (!prior.HasValue) return (null, null);

        bool lowerIsBetter = PerformanceScorer.IsLowerBetterMetric(metricKey);
        double delta = current - prior.Value;

        if (lowerIsBetter)
        {
            double absImprovement = prior.Value - current;
            double? pct = prior.Value > 0 ? (absImprovement / prior.Value) * 100.0 : null;
            return (absImprovement, pct);
        }

        double? percent = prior.Value != 0 ? (delta / prior.Value) * 100.0 : null;
        return (delta, percent);
    }
}
