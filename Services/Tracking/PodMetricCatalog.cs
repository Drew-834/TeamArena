using System.Globalization;

namespace GameScoreboard.Services.Tracking;

public static class PodMetricCatalog
{
    private static readonly string[] _coreMetrics =
    {
        "RPH",
        "AppEff",
        "PMEff",
        "Surveys",
        "WarrantyAttach"
    };

    private static readonly string[] _pcDepartmentMetrics =
    {
        "RPH",
        "AppEff",
        "PMEff",
        "Surveys",
        "WarrantyAttach",
        "PCBasket",
        "Office"
    };

    private static readonly string[] _htDepartmentMetrics =
    {
        "RPH",
        "AppEff",
        "PMEff",
        "Surveys",
        "WarrantyAttach",
        "HTBasket",
        "ServAttach"
    };

    private static readonly string[] _allMetrics =
    {
        "RPH",
        "AppEff",
        "PMEff",
        "Surveys",
        "WarrantyAttach",
        "PCBasket",
        "HTBasket",
        "ServAttach",
        "Office"
    };

    private static readonly HashSet<string> _regularDepartments = new(StringComparer.OrdinalIgnoreCase)
    {
        "front",
        "computers",
        "warehouse",
        "store"
    };

    private static readonly Dictionary<string, string[]> _metricAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ServAttach"] = new[] { "ServAttach", "AccAttach" },
        ["AccAttach"] = new[] { "AccAttach", "ServAttach" }
    };

    public static IReadOnlyList<string> CoreMetrics => _coreMetrics;

    public static IReadOnlyList<string> AllMetrics => _allMetrics;

    public static IReadOnlyDictionary<string, int> MetricWeights { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["RPH"] = 7,
        ["AppEff"] = 7,
        ["PMEff"] = 5,
        ["WarrantyAttach"] = 5,
        ["Surveys"] = 3,
        ["PCBasket"] = 5,
        ["HTBasket"] = 5,
        ["ServAttach"] = 5,
        ["Office"] = 5
    };

    public static IReadOnlyDictionary<string, double> MetricTargets { get; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        ["RPH"] = 920.0,
        ["AppEff"] = 6500.0,
        ["PMEff"] = 4000.0,
        ["Surveys"] = 4.6,
        ["WarrantyAttach"] = 10.0,
        ["PCBasket"] = 140.0,
        ["HTBasket"] = 300.0,
        ["ServAttach"] = 25.0,
        ["Office"] = 25.0,
        ["AccAttach"] = 25.0
    };

    public static bool IsPodDepartment(string? department) =>
        !string.IsNullOrWhiteSpace(department) && !_regularDepartments.Contains(department);

    public static bool IsHTPod(string? department)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            return false;
        }

        return department.Contains("HT", StringComparison.OrdinalIgnoreCase)
            || department.Contains("Home Theater", StringComparison.OrdinalIgnoreCase)
            || (department.Contains("Retail Programs", StringComparison.OrdinalIgnoreCase)
                && !department.Contains("Center Store", StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<string> GetApplicableMetrics(string? department)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            return _allMetrics;
        }

        return IsHTPod(department) ? _htDepartmentMetrics : _pcDepartmentMetrics;
    }

    public static IReadOnlyList<string> GetPublicDisplayMetrics(string? department) => _coreMetrics;

    public static IEnumerable<string> GetLookupKeys(string metricKey, string? department = null)
    {
        if (_metricAliases.TryGetValue(metricKey, out var aliases))
        {
            foreach (var alias in aliases)
            {
                yield return alias;
            }

            yield break;
        }

        yield return metricKey;
    }

    public static double GetTarget(string metricKey) =>
        MetricTargets.TryGetValue(metricKey, out var target) ? target : 0.0;

    public static int GetWeight(string metricKey) =>
        MetricWeights.TryGetValue(metricKey, out var weight) ? weight : 0;

    public static bool IsLowerBetter(string metricKey) =>
        metricKey.Equals("AppEff", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("PMEff", StringComparison.OrdinalIgnoreCase);

    public static bool IsPercentageMetric(string metricKey) =>
        metricKey.Equals("WarrantyAttach", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("ServAttach", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("Office", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("AccAttach", StringComparison.OrdinalIgnoreCase);

    public static bool IsCurrencyMetric(string metricKey) =>
        metricKey.Equals("RPH", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("AppEff", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("PMEff", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("PCBasket", StringComparison.OrdinalIgnoreCase)
        || metricKey.Equals("HTBasket", StringComparison.OrdinalIgnoreCase);

    public static bool IsRatingMetric(string metricKey) =>
        metricKey.Equals("Surveys", StringComparison.OrdinalIgnoreCase);

    public static bool IsIgnoredNumericValue(string metricKey, double value)
    {
        if (IsLowerBetter(metricKey) && value >= 20000)
        {
            return true;
        }

        return false;
    }

    public static string FormatMetricValue(double value, string metricKey)
    {
        if (IsPercentageMetric(metricKey))
        {
            return value.ToString("F1", CultureInfo.InvariantCulture) + "%";
        }

        if (IsCurrencyMetric(metricKey))
        {
            return "$" + value.ToString("N0", CultureInfo.InvariantCulture);
        }

        if (IsRatingMetric(metricKey))
        {
            return value.ToString("F1", CultureInfo.InvariantCulture);
        }

        return value.ToString("N0", CultureInfo.InvariantCulture);
    }
}
