using GameScoreboard.Models;

namespace GameScoreboard.Services.Tracking;

public enum PerformanceReviewTier
{
    NoData,
    Concern,
    Fair,
    Strong,
    Exceptional
}

public record KpiReviewSlide(
    string MetricKey,
    string DisplayName,
    int Weight,
    double? CurrentValue,
    double? PriorValue,
    double Goal,
    double Score,
    double? PriorScore,
    double GoalProgressPercent,
    double? DeltaVsPriorAbsolute,
    double? DeltaVsPriorPercent,
    PerformanceReviewTier Tier);

public record PerformanceReviewResult(
    TeamMember Member,
    string CurrentPeriod,
    string? PriorPeriod,
    IReadOnlyList<KpiReviewSlide> Slides,
    double WeightedScore,
    double? PriorWeightedScore,
    double? ScoreDeltaVsPrior,
    int? PodRank,
    int PodMemberCount,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> FocusAreas);

public enum PerformanceReviewPhase
{
    Picker,
    Loading,
    KpiSlide,
    TotalReveal,
    Recap
}
