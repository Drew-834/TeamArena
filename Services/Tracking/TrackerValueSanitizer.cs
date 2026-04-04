using System.Globalization;

namespace GameScoreboard.Services.Tracking;

public static class TrackerValueSanitizer
{
    public static string CleanCurrency(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        raw = raw.Trim();
        if (IsEmptyMetricCell(raw))
        {
            return string.Empty;
        }

        raw = raw.Replace("$", string.Empty)
            .Replace(",", string.Empty)
            .Trim();

        if (raw.StartsWith("(") && raw.EndsWith(")", StringComparison.Ordinal))
        {
            return "-" + raw.TrimStart('(').TrimEnd(')').Trim();
        }

        return raw;
    }

    public static string CleanPercentage(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        raw = raw.Trim();
        if (IsEmptyMetricCell(raw))
        {
            return string.Empty;
        }

        return raw.Replace("%", string.Empty).Trim();
    }

    public static bool IsEmptyMetricCell(string raw) =>
        raw.Equals("On Track", StringComparison.OrdinalIgnoreCase)
        || raw.Equals("Off Track", StringComparison.OrdinalIgnoreCase)
        || raw.Equals("#N/A", StringComparison.OrdinalIgnoreCase)
        || raw.Equals("#REF!", StringComparison.OrdinalIgnoreCase);

    public static double? ParseNullableDouble(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (TryParseDouble(raw, out var value))
        {
            return value;
        }

        return null;
    }

    public static bool TryParseDouble(string? raw, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var normalized = raw.Trim()
            .Replace("$", string.Empty)
            .Replace("%", string.Empty)
            .Replace(",", string.Empty)
            .Trim();

        if (normalized.StartsWith("(") && normalized.EndsWith(")", StringComparison.Ordinal))
        {
            normalized = "-" + normalized.TrimStart('(').TrimEnd(')').Trim();
        }

        return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
