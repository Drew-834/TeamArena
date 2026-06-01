using System.Globalization;

namespace GameScoreboard.Services.Tracking;

public static class PeriodHelper
{
    public static string? GetLatestPeriod(IEnumerable<string?> periodStrings)
    {
        return periodStrings
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => new { Period = p!, EndDate = GetPeriodEndDate(p!) })
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefault()
            ?.Period;
    }

    public static string? GetPriorPeriod(IEnumerable<string?> periodStrings, string currentPeriod)
    {
        return periodStrings
            .Where(p => !string.IsNullOrWhiteSpace(p)
                && !string.Equals(p, currentPeriod, StringComparison.OrdinalIgnoreCase))
            .Select(p => new { Period = p!, EndDate = GetPeriodEndDate(p!) })
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefault()
            ?.Period;
    }

    public static DateTime GetPeriodEndDate(string period)
    {
        if (string.IsNullOrWhiteSpace(period))
            return DateTime.MinValue;

        if (DateTime.TryParseExact(period, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var isoDate))
            return isoDate;

        var parts = period.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2
            && DateTime.TryParseExact("01-" + parts[1], "dd-MMM yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var monthStartDate))
        {
            return parts[0].Equals("Mid", StringComparison.OrdinalIgnoreCase)
                ? monthStartDate.AddDays(14)
                : monthStartDate.AddMonths(1).AddDays(-1);
        }

        return DateTime.TryParse(period, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback)
            ? fallback
            : DateTime.MinValue;
    }

    public static string FormatPeriodDisplay(string? period)
    {
        if (string.IsNullOrEmpty(period)) return "No Data Available";
        if (DateTime.TryParseExact(period, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
            return dt.ToString("MMMM d, yyyy");
        return period;
    }
}
