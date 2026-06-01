using GameScoreboard.Models;

namespace GameScoreboard.Services.Tracking;

public static class PerformanceScorer
{
    public static bool IsPodEfficiency(string metricKey) =>
        metricKey.Equals("AppEff", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("PMEff", StringComparison.OrdinalIgnoreCase);

    public static bool IsLowerBetterMetric(string metricKey) =>
        metricKey.Equals("Awk", StringComparison.OrdinalIgnoreCase)
        || PodMetricCatalog.IsLowerBetter(metricKey);

    public static double? GetMemberGoal(TeamMember? member, string metricKey)
    {
        if (member != null)
        {
            if (metricKey.Equals("RPH", StringComparison.OrdinalIgnoreCase) && member.RphGoal.HasValue)
                return member.RphGoal;
            if (metricKey.Equals("AppEff", StringComparison.OrdinalIgnoreCase) && member.AppEffGoal.HasValue)
                return member.AppEffGoal;
            if (metricKey.Equals("PMEff", StringComparison.OrdinalIgnoreCase) && member.PmEffGoal.HasValue)
                return member.PmEffGoal;
            if (metricKey.Equals("WarrantyAttach", StringComparison.OrdinalIgnoreCase) && member.WarrantyAttachGoal.HasValue)
                return member.WarrantyAttachGoal;
            if ((metricKey.Equals("AccAttach", StringComparison.OrdinalIgnoreCase)
                 || metricKey.Equals("ServAttach", StringComparison.OrdinalIgnoreCase))
                && member.AccAttachGoal.HasValue)
                return member.AccAttachGoal;
        }

        if (member != null && PodMetricCatalog.IsPodDepartment(member.Department))
            return PodMetricCatalog.GetTarget(metricKey);

        return metricKey switch
        {
            "PMAttach" => 25.0,
            "M365Attach" => 30.0,
            "GSP" => 20.0,
            _ => null
        };
    }

    public static double CalculatePerformanceScore(
        string metricKey,
        double numericValue,
        IReadOnlyDictionary<string, double> teamAverages,
        TeamMember? member = null)
    {
        bool lowerIsBetter = IsLowerBetterMetric(metricKey);

        if ((metricKey.Equals("Surveys", StringComparison.OrdinalIgnoreCase)
             || metricKey.Equals("5Star", StringComparison.OrdinalIgnoreCase))
            && numericValue == 0)
            return 15;

        if (IsPodEfficiency(metricKey) && numericValue >= 20000)
            return 0;

        double? goal = GetMemberGoal(member, metricKey);

        double baseScoreTarget;
        if (goal.HasValue && goal.Value > 0)
            baseScoreTarget = goal.Value;
        else if (teamAverages.TryGetValue(metricKey, out double average) && average > 0)
            baseScoreTarget = average;
        else
            return numericValue > 0 ? 50 : 0;

        if (baseScoreTarget <= 0) return 0;

        double score;
        if (lowerIsBetter)
        {
            if (numericValue <= 0) return 100;
            score = (baseScoreTarget / numericValue) * 100.0;
        }
        else
        {
            score = (numericValue / baseScoreTarget) * 100.0;
        }

        if (teamAverages.TryGetValue(metricKey, out double teamAvg) && teamAvg > 0)
        {
            bool aboveAvg = lowerIsBetter
                ? numericValue < teamAvg
                : numericValue > teamAvg;
            score += aboveAvg ? 5.0 : -5.0;
        }

        if (score > 100)
        {
            double excess = score - 100;
            double bonus = 15.0 * Math.Log(1 + excess / 20.0);
            score = 100 + Math.Min(bonus, 25.0);
        }

        return Math.Clamp(score, 0, 125);
    }

    public static PerformanceReviewTier GetTier(double score, bool hasData)
    {
        if (!hasData) return PerformanceReviewTier.NoData;
        if (score >= 100) return PerformanceReviewTier.Exceptional;
        if (score >= 80) return PerformanceReviewTier.Strong;
        if (score >= 60) return PerformanceReviewTier.Fair;
        return PerformanceReviewTier.Concern;
    }

    public static string GetTierCssClass(PerformanceReviewTier tier) => tier switch
    {
        PerformanceReviewTier.Exceptional => "review-tier-exceptional",
        PerformanceReviewTier.Strong => "review-tier-strong",
        PerformanceReviewTier.Fair => "review-tier-fair",
        PerformanceReviewTier.Concern => "review-tier-concern",
        _ => "review-tier-nodata"
    };

    public static string GetTierSoundMethod(PerformanceReviewTier tier) => tier switch
    {
        PerformanceReviewTier.Exceptional => "eldenAudio.playTriumph",
        PerformanceReviewTier.Strong => "eldenAudio.playAffirm",
        PerformanceReviewTier.Fair => "eldenAudio.playNeutral",
        PerformanceReviewTier.Concern => "eldenAudio.playConcern",
        _ => "eldenAudio.playNeutral"
    };

    public static double GetGoalProgressPercent(string metricKey, double value, double goal)
    {
        if (goal <= 0) return 0;
        if (IsLowerBetterMetric(metricKey))
            return value > 0 ? Math.Clamp((goal / value) * 100.0, 0, 125) : 0;
        return Math.Clamp((value / goal) * 100.0, 0, 125);
    }
}
