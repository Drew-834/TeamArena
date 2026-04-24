namespace GameScoreboard.Services;

/// <summary>Visual tiers for <see cref="Models.TeamMember.CompanyRank"/> (parser Rank column; lower = better).</summary>
public static class CompanyRankTiers
{
    /// <summary>CSS class suffix for champion cards; empty if no extra styling.</summary>
    public static string GetChampionCardTierClass(int? companyRank) => companyRank switch
    {
        null => "",
        int r when r is >= 1 and <= 125 => "elden-character-card--rank-prismatic",
        int r when r is >= 126 and <= 250 => "elden-character-card--rank-gold",
        int r when r is >= 251 and <= 500 => "elden-character-card--rank-silver",
        _ => ""
    };

    /// <summary>Bar fill 0-100: better rank (lower R) = fuller; empty for R &gt; 1250; R=1 full, R=1250 empty.</summary>
    public static double GetRankBarPercent(int? companyRank)
    {
        if (!companyRank.HasValue || companyRank.Value < 1) return 0;
        int r = companyRank.Value;
        if (r > 1250) return 0;
        return 100.0 * (1250.0 - r) / 1249.0;
    }
}
