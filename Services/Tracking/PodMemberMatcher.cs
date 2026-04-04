using GameScoreboard.Models;

namespace GameScoreboard.Services.Tracking;

public enum PodMemberMatchKind
{
    Exact,
    Fuzzy,
    Moved,
    NewMember,
    Ambiguous,
    Unmatched
}

public sealed record PodMemberMatchResult(
    PodMemberMatchKind Kind,
    TeamMember? Member,
    string ExcelName,
    string TargetDepartment,
    string? PreviousDepartment = null,
    IReadOnlyList<string>? Candidates = null,
    string? Notes = null)
{
    public bool IsMatch => Member != null && Kind is PodMemberMatchKind.Exact or PodMemberMatchKind.Fuzzy or PodMemberMatchKind.Moved;
}

public static class PodMemberMatcher
{
    public static PodMemberMatchResult Match(
        string excelName,
        string targetDepartment,
        IEnumerable<TeamMember> currentDepartmentMembers,
        IEnumerable<TeamMember> allPodMembers)
    {
        var current = currentDepartmentMembers
            .Where(m => m != null)
            .DistinctBy(m => m.Id)
            .ToList();

        var everyone = allPodMembers
            .Where(m => m != null)
            .DistinctBy(m => m.Id)
            .ToList();

        if (string.IsNullOrWhiteSpace(excelName))
        {
            return new PodMemberMatchResult(PodMemberMatchKind.Unmatched, null, excelName, targetDepartment, Notes: "Name was blank.");
        }

        var exactCurrent = current.FirstOrDefault(m => NameEquals(m.Name, excelName));
        if (exactCurrent != null)
        {
            return new PodMemberMatchResult(PodMemberMatchKind.Exact, exactCurrent, excelName, targetDepartment);
        }

        var exactOther = everyone
            .Where(m => !m.Department.Equals(targetDepartment, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(m => NameEquals(m.Name, excelName));
        if (exactOther != null)
        {
            return new PodMemberMatchResult(PodMemberMatchKind.Moved, exactOther, excelName, targetDepartment, exactOther.Department);
        }

        var currentCandidates = FindCandidates(excelName, current, allowFirstNameOnly: true, requireLastNameOverlap: false);
        if (currentCandidates.Count == 1)
        {
            return new PodMemberMatchResult(PodMemberMatchKind.Fuzzy, currentCandidates[0], excelName, targetDepartment, Notes: "Resolved inside current team.");
        }

        if (currentCandidates.Count > 1)
        {
            return new PodMemberMatchResult(
                PodMemberMatchKind.Ambiguous,
                null,
                excelName,
                targetDepartment,
                Candidates: currentCandidates.Select(m => m.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Notes: "Multiple current-team candidates matched.");
        }

        var movedCandidates = FindCandidates(excelName, everyone, allowFirstNameOnly: false, requireLastNameOverlap: true)
            .Where(m => !m.Department.Equals(targetDepartment, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (movedCandidates.Count == 1)
        {
            return new PodMemberMatchResult(
                PodMemberMatchKind.Moved,
                movedCandidates[0],
                excelName,
                targetDepartment,
                movedCandidates[0].Department,
                Notes: "Matched strongly against a different team.");
        }

        if (movedCandidates.Count > 1)
        {
            return new PodMemberMatchResult(
                PodMemberMatchKind.Ambiguous,
                null,
                excelName,
                targetDepartment,
                Candidates: movedCandidates.Select(m => m.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Notes: "Multiple cross-team candidates matched.");
        }

        return new PodMemberMatchResult(PodMemberMatchKind.NewMember, null, excelName, targetDepartment, Notes: "No existing member matched strongly.");
    }

    public static string NormalizeName(string? name) =>
        (name ?? string.Empty).Trim().ToLowerInvariant();

    private static List<TeamMember> FindCandidates(
        string excelName,
        IReadOnlyList<TeamMember> members,
        bool allowFirstNameOnly,
        bool requireLastNameOverlap)
    {
        var candidates = new List<TeamMember>();
        var excelTokens = Tokenize(excelName);
        if (excelTokens.Count == 0)
        {
            return candidates;
        }

        var firstName = excelTokens[0];
        var lastName = excelTokens.Count > 1 ? excelTokens[^1] : string.Empty;

        foreach (var member in members)
        {
            var memberTokens = Tokenize(member.Name);
            if (memberTokens.Count == 0)
            {
                continue;
            }

            var memberFirst = memberTokens[0];
            var memberLast = memberTokens.Count > 1 ? memberTokens[^1] : string.Empty;

            if (firstName.Equals(memberFirst, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(lastName)
                && lastName.Equals(memberLast, StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(member);
                continue;
            }

            if (!requireLastNameOverlap
                && allowFirstNameOnly
                && firstName.Equals(memberFirst, StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(member);
                continue;
            }

            if (!string.IsNullOrEmpty(lastName)
                && member.Name.Contains(firstName, StringComparison.OrdinalIgnoreCase)
                && member.Name.Contains(lastName, StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(member);
                continue;
            }

            if (LooksLikeStrongFuzzyMatch(excelName, member.Name, requireLastNameOverlap))
            {
                candidates.Add(member);
            }
        }

        return candidates
            .DistinctBy(m => m.Id)
            .ToList();
    }

    private static bool NameEquals(string left, string right) =>
        left.Equals(right, StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeStrongFuzzyMatch(string excelName, string systemName, bool requireLastNameOverlap)
    {
        var excelTokens = Tokenize(excelName);
        var systemTokens = Tokenize(systemName);
        if (excelTokens.Count < 2 || systemTokens.Count < 2)
        {
            return false;
        }

        if (requireLastNameOverlap
            && !excelTokens[^1].Equals(systemTokens[^1], StringComparison.OrdinalIgnoreCase)
            && !excelTokens[^1].Contains(systemTokens[^1], StringComparison.OrdinalIgnoreCase)
            && !systemTokens[^1].Contains(excelTokens[^1], StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalizedExcel = string.Join(' ', excelTokens);
        var normalizedSystem = string.Join(' ', systemTokens);

        if (Math.Abs(normalizedExcel.Length - normalizedSystem.Length) > 3)
        {
            return false;
        }

        var distance = LevenshteinDistance(normalizedExcel, normalizedSystem);
        var maxLen = Math.Max(normalizedExcel.Length, normalizedSystem.Length);
        return distance <= 2 && distance < maxLen * 0.2;
    }

    private static List<string> Tokenize(string? value) =>
        (value ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    private static int LevenshteinDistance(string source, string target)
    {
        var matrix = new int[source.Length + 1, target.Length + 1];
        for (var i = 0; i <= source.Length; i++)
        {
            matrix[i, 0] = i;
        }

        for (var j = 0; j <= target.Length; j++)
        {
            matrix[0, j] = j;
        }

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }
}
