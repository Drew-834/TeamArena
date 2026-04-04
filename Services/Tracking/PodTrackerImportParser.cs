using GameScoreboard.Models;

namespace GameScoreboard.Services.Tracking;

public enum ImportLogSeverity
{
    Info,
    Warning,
    Error
}

public sealed record ImportLogEntry(ImportLogSeverity Severity, string Message)
{
    public string ToDisplayString()
    {
        var prefix = Severity switch
        {
            ImportLogSeverity.Info => "✓",
            ImportLogSeverity.Warning => "⚠",
            ImportLogSeverity.Error => "✗",
            _ => "•"
        };

        return $"{prefix} {Message}";
    }
}

public sealed class PodTrackerParseResult
{
    public bool IsNewFormat { get; init; }

    public string StatusMessage { get; set; } = string.Empty;

    public List<ImportLogEntry> Logs { get; } = new();

    public List<PodImportSection> Pods { get; } = new();

    public int MatchedCount => Pods.Sum(p => p.MatchedMembers.Count);

    public int NewCount => Pods.Sum(p => p.NewMembers.Count);

    public int UnmatchedCount => Pods.Sum(p => p.UnmatchedNames.Count);

    public bool HasRows => Pods.Any(p => p.Rows.Any());
}

public sealed class PodImportSection
{
    public required string ExcelName { get; init; }

    public required string SystemName { get; init; }

    public bool IsNew { get; init; }

    public Dictionary<string, int> DepartmentSpecificColumns { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<PodImportRow> Rows { get; } = new();

    public List<string> MatchedMembers { get; } = new();

    public List<string> NewMembers { get; } = new();

    public List<string> UnmatchedNames { get; } = new();
}

public sealed class PodImportRow
{
    public int RowNumber { get; init; }

    public required string MemberName { get; init; }

    public required PodMemberMatchResult Match { get; init; }

    public required Dictionary<string, string> MetricValues { get; init; }
}

public sealed class PodTrackerImportParser
{
    private static readonly Dictionary<string, string> _podAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LUIS-DI/HT/Mobile"] = "Luis the Beast",
        ["Matt Nelsen-PC/CA"] = "Matt Nelsen-PC/CA",
        ["Matt Nelsen-PC/APPL"] = "Matt Nelsen-PC/APPL",
        ["Home Theater-HT/CA"] = "Home Theater-HT/CA",
        ["Retail Programs-HT"] = "Retail Programs-HT",
        ["Retail Programs-Center Store"] = "Retail Programs-Center Store",
        ["Drew Carillo"] = "Drews Crew-Computing",
        ["Drew Carrillo"] = "Drews Crew-Computing",
        ["Jaime Torres"] = "Jaime Torres",
        ["Luis Ramos"] = "Luis Ramos"
    };

    public PodTrackerParseResult Parse(string pastedData, IReadOnlyCollection<TeamMember> allPodMembers)
    {
        var result = new PodTrackerParseResult();
        var lines = pastedData
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        if (lines.Length == 0)
        {
            result.StatusMessage = "Paste area is empty.";
            result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Error, result.StatusMessage));
            return result;
        }

        if (!LooksLikeNewFormat(lines))
        {
            result.StatusMessage = "Input did not match the new dashboard tracker layout.";
            result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Warning, result.StatusMessage));
            return result;
        }

        result = new PodTrackerParseResult
        {
            IsNewFormat = true
        };
        result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Info, "Using NEW format parser (dashboard output)."));

        if (!TryFindLayout(lines, out var layout, out var headerRowIndex))
        {
            result.StatusMessage = "Could not locate the dashboard header row.";
            result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Error, result.StatusMessage));
            return result;
        }

        result.Logs.Add(new ImportLogEntry(
            ImportLogSeverity.Info,
            $"Header found at row {headerRowIndex + 1}. Core columns: Employee={layout.NameColumn}, RPH={layout.RphColumn}, AppEff={layout.AppEffColumn}, PMEff={layout.PmEffColumn}, Surveys={layout.SurveysColumn}, Warranty={layout.WarrantyColumn}."));

        PodImportSection? currentPod = null;
        for (var rowIndex = headerRowIndex + 1; rowIndex < lines.Length; rowIndex++)
        {
            var cells = lines[rowIndex].Split('\t');
            if (cells.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var rowNumber = rowIndex + 1;
            var nameCell = SafeGet(cells, layout.NameColumn).Trim();
            if (string.IsNullOrWhiteSpace(nameCell))
            {
                continue;
            }

            if (ShouldSkipMetadataRow(nameCell))
            {
                continue;
            }

            if (IsPodHeaderRow(nameCell, cells))
            {
                var systemName = ResolvePodName(nameCell, allPodMembers);
                currentPod = new PodImportSection
                {
                    ExcelName = nameCell,
                    SystemName = systemName,
                    IsNew = !PodExistsInSystem(systemName, allPodMembers)
                };

                foreach (var column in ResolveDepartmentSpecificColumns(cells, layout, systemName))
                {
                    currentPod.DepartmentSpecificColumns[column.Key] = column.Value;
                }

                result.Pods.Add(currentPod);

                result.Logs.Add(new ImportLogEntry(
                    ImportLogSeverity.Info,
                    nameCell.Equals(systemName, StringComparison.OrdinalIgnoreCase)
                        ? $"Pod '{nameCell}' mapped to '{systemName}'."
                        : $"Pod '{nameCell}' resolved to '{systemName}'."));

                if (currentPod.DepartmentSpecificColumns.Count > 0)
                {
                    result.Logs.Add(new ImportLogEntry(
                        ImportLogSeverity.Info,
                        $"Department-specific columns: {string.Join(", ", currentPod.DepartmentSpecificColumns.Select(kv => $"{kv.Key}={kv.Value}"))}."));
                }

                continue;
            }

            if (currentPod == null)
            {
                result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Warning, $"Row {rowNumber}: skipped '{nameCell}' because no pod header had been detected yet."));
                continue;
            }

            if (IsNARow(cells))
            {
                result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Warning, $"Row {rowNumber}: skipped '{nameCell}' because the row is mostly #N/A."));
                continue;
            }

            var currentDepartmentMembers = allPodMembers
                .Where(m => m.Department.Equals(currentPod.SystemName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var match = PodMemberMatcher.Match(nameCell, currentPod.SystemName, currentDepartmentMembers, allPodMembers);
            switch (match.Kind)
            {
                case PodMemberMatchKind.Ambiguous:
                    currentPod.UnmatchedNames.Add(nameCell);
                    result.Logs.Add(new ImportLogEntry(
                        ImportLogSeverity.Warning,
                        $"Row {rowNumber}: ambiguous match for '{nameCell}' ({string.Join(", ", match.Candidates ?? Array.Empty<string>())})."));
                    continue;

                case PodMemberMatchKind.Unmatched:
                    currentPod.UnmatchedNames.Add(nameCell);
                    result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Warning, $"Row {rowNumber}: no reliable match for '{nameCell}'."));
                    continue;

                case PodMemberMatchKind.NewMember:
                    currentPod.NewMembers.Add(nameCell);
                    result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Info, $"Row {rowNumber}: '{nameCell}' will be created in '{currentPod.SystemName}'."));
                    break;

                case PodMemberMatchKind.Moved:
                    currentPod.MatchedMembers.Add(nameCell);
                    result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Warning, $"Row {rowNumber}: '{nameCell}' matched an existing member from '{match.PreviousDepartment}' and will move to '{currentPod.SystemName}'."));
                    break;

                case PodMemberMatchKind.Fuzzy:
                    currentPod.MatchedMembers.Add(nameCell);
                    result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Warning, $"Row {rowNumber}: '{nameCell}' matched existing member '{match.Member?.Name}' using relaxed matching."));
                    break;

                default:
                    currentPod.MatchedMembers.Add(nameCell);
                    break;
            }

            var values = ParseMetricValues(cells, layout, currentPod);
            currentPod.Rows.Add(new PodImportRow
            {
                RowNumber = rowNumber,
                MemberName = nameCell,
                Match = match,
                MetricValues = values
            });

            result.Logs.Add(new ImportLogEntry(
                ImportLogSeverity.Info,
                $"Row {rowNumber}: Name={nameCell} | Pod={currentPod.SystemName} | RPH={values["RPH"]} | AppEff={values["AppEff"]} | PMEff={values["PMEff"]} | Surveys={values["Surveys"]} | Warranty={values["WarrantyAttach"]}."));
        }

        result.StatusMessage = result.HasRows
            ? $"Parsed {result.MatchedCount + result.NewCount} members across {result.Pods.Count} teams."
            : "No member rows were parsed from the pasted dashboard.";

        if (!result.HasRows)
        {
            result.Logs.Add(new ImportLogEntry(ImportLogSeverity.Warning, result.StatusMessage));
        }

        return result;
    }

    private static bool LooksLikeNewFormat(IEnumerable<string> lines) =>
        lines.Any(l => l.Contains("Employee Relative Performance Dashboard", StringComparison.OrdinalIgnoreCase)
            || l.Contains("Relative Performance Dashboard Output", StringComparison.OrdinalIgnoreCase));

    private static bool TryFindLayout(string[] lines, out DashboardColumnLayout layout, out int headerRowIndex)
    {
        layout = default;
        headerRowIndex = -1;

        for (var i = 0; i < Math.Min(lines.Length, 20); i++)
        {
            var cells = lines[i].Split('\t');
            var employeeIndex = Array.FindIndex(cells, c => c.Trim().Equals("Employee", StringComparison.OrdinalIgnoreCase));
            if (employeeIndex < 0)
            {
                continue;
            }

            var rphColumn = FindColumn(cells, "Rev Per Hour");
            var appsColumn = FindColumn(cells, "Apps / App Eff");
            var pmsColumn = FindColumn(cells, "PMs / PM Eff");
            var surveysColumn = FindColumn(cells, "Surveys / 5-Star", "Surveys / 5 Star");
            var warrantyColumn = FindColumn(cells, "Warranty Attach");
            var deptSpecificColumn = FindColumn(cells, "Dept Specific");
            var rankColumn = FindColumn(cells, "Rank");
            var kpisHitColumn = FindColumn(cells, "KPIs Hit");

            if (rphColumn < 0 || appsColumn < 0 || pmsColumn < 0 || surveysColumn < 0 || warrantyColumn < 0)
            {
                continue;
            }

            layout = new DashboardColumnLayout(
                employeeIndex,
                rphColumn,
                appsColumn + 1,
                pmsColumn + 1,
                surveysColumn + 1,
                warrantyColumn,
                deptSpecificColumn,
                rankColumn,
                kpisHitColumn);

            headerRowIndex = i;
            return true;
        }

        return false;
    }

    private static Dictionary<string, int> ResolveDepartmentSpecificColumns(string[] cells, DashboardColumnLayout layout, string department)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (layout.DepartmentSpecificStart < 0)
        {
            return result;
        }

        var endExclusive = layout.RankColumn > layout.DepartmentSpecificStart ? layout.RankColumn : cells.Length;
        for (var index = layout.DepartmentSpecificStart; index < endExclusive && index < cells.Length; index++)
        {
            var header = cells[index].Trim();
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            if (header.Contains("PC Basket", StringComparison.OrdinalIgnoreCase))
            {
                result["PCBasket"] = index;
            }
            else if (header.Contains("HT Basket", StringComparison.OrdinalIgnoreCase))
            {
                result["HTBasket"] = index;
            }
            else if (header.Contains("Serv", StringComparison.OrdinalIgnoreCase))
            {
                result["ServAttach"] = index;
            }
            else if (header.Contains("Office", StringComparison.OrdinalIgnoreCase))
            {
                result["Office"] = index;
            }
        }

        if (!result.Any())
        {
            if (PodMetricCatalog.IsHTPod(department))
            {
                result["HTBasket"] = layout.DepartmentSpecificStart;
                result["ServAttach"] = layout.DepartmentSpecificStart + 1;
            }
            else
            {
                result["PCBasket"] = layout.DepartmentSpecificStart;
                result["Office"] = layout.DepartmentSpecificStart + 1;
            }
        }

        return result;
    }

    private static Dictionary<string, string> ParseMetricValues(string[] cells, DashboardColumnLayout layout, PodImportSection currentPod)
    {
        var values = PodMetricCatalog.AllMetrics.ToDictionary(key => key, _ => string.Empty, StringComparer.OrdinalIgnoreCase);
        values["RPH"] = TrackerValueSanitizer.CleanCurrency(SafeGet(cells, layout.RphColumn));
        values["AppEff"] = TrackerValueSanitizer.CleanCurrency(SafeGet(cells, layout.AppEffColumn));
        values["PMEff"] = TrackerValueSanitizer.CleanCurrency(SafeGet(cells, layout.PmEffColumn));
        values["Surveys"] = TrackerValueSanitizer.CleanPercentage(SafeGet(cells, layout.SurveysColumn));
        values["WarrantyAttach"] = TrackerValueSanitizer.CleanPercentage(SafeGet(cells, layout.WarrantyColumn));

        foreach (var metricKey in currentPod.DepartmentSpecificColumns.Keys)
        {
            var value = SafeGet(cells, currentPod.DepartmentSpecificColumns[metricKey]);
            values[metricKey] = PodMetricCatalog.IsPercentageMetric(metricKey)
                ? TrackerValueSanitizer.CleanPercentage(value)
                : TrackerValueSanitizer.CleanCurrency(value);
        }

        return values;
    }

    private static bool IsPodHeaderRow(string nameCell, string[] cells)
    {
        if (IsKnownPodHeader(nameCell))
        {
            return true;
        }

        var actualGoalTrackCells = cells.Count(c =>
        {
            var trimmed = c.Trim();
            return trimmed.Equals("Actaul", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Actual", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Goal", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Track", StringComparison.OrdinalIgnoreCase);
        });

        if (actualGoalTrackCells >= 4)
        {
            return true;
        }

        return nameCell.Contains("PC/", StringComparison.OrdinalIgnoreCase)
            || nameCell.Contains("HT/", StringComparison.OrdinalIgnoreCase)
            || nameCell.Contains("Center Store", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsKnownPodHeader(string nameCell) =>
        _podAliases.ContainsKey(nameCell)
        || nameCell.EndsWith("PC/CA", StringComparison.OrdinalIgnoreCase)
        || nameCell.EndsWith("PC/APPL", StringComparison.OrdinalIgnoreCase)
        || nameCell.EndsWith("HT/CA", StringComparison.OrdinalIgnoreCase)
        || nameCell.EndsWith("HT/APPL", StringComparison.OrdinalIgnoreCase)
        || nameCell.Contains("Retail Programs", StringComparison.OrdinalIgnoreCase)
        || nameCell.Contains("Home Theater", StringComparison.OrdinalIgnoreCase);

    private static string ResolvePodName(string excelName, IReadOnlyCollection<TeamMember> allPodMembers)
    {
        if (_podAliases.TryGetValue(excelName, out var systemName))
        {
            return systemName;
        }

        var exact = allPodMembers
            .Select(m => m.Department)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(d => d.Equals(excelName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(exact))
        {
            return exact;
        }

        return excelName;
    }

    private static bool PodExistsInSystem(string podName, IReadOnlyCollection<TeamMember> allPodMembers) =>
        allPodMembers.Any(m => m.Department.Equals(podName, StringComparison.OrdinalIgnoreCase));

    private static bool ShouldSkipMetadataRow(string nameCell)
    {
        if (nameCell.Equals("1411", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return double.TryParse(nameCell.Replace(",", string.Empty), out _);
    }

    private static bool IsNARow(string[] cells) =>
        cells.Count(c => c.Trim().Equals("#N/A", StringComparison.OrdinalIgnoreCase)) >= 3;

    private static string SafeGet(string[] cells, int index) =>
        index >= 0 && index < cells.Length ? cells[index] ?? string.Empty : string.Empty;

    private static int FindColumn(string[] cells, params string[] labels)
    {
        for (var i = 0; i < cells.Length; i++)
        {
            var trimmed = cells[i].Trim();
            if (labels.Any(label => trimmed.Contains(label, StringComparison.OrdinalIgnoreCase)))
            {
                return i;
            }
        }

        return -1;
    }

    private readonly record struct DashboardColumnLayout(
        int NameColumn,
        int RphColumn,
        int AppEffColumn,
        int PmEffColumn,
        int SurveysColumn,
        int WarrantyColumn,
        int DepartmentSpecificStart,
        int RankColumn,
        int KpisHitColumn);
}
