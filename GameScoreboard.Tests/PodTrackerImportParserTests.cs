using GameScoreboard.Models;
using GameScoreboard.Services.Tracking;

namespace GameScoreboard.Tests;

public class PodTrackerImportParserTests
{
    private readonly PodTrackerImportParser _parser = new();

    [Fact]
    public void Parse_NewDashboardFormat_UsesStructuralColumnsForWarrantyAndDeptSpecificValues()
    {
        var existingMember = new TeamMember
        {
            Id = 7,
            Name = "Joao Aguiar de Pinho Pacheco",
            Department = "Old Pod",
            AvatarUrl = "images/avatars/avatar4.png"
        };

        var staleMember = new TeamMember
        {
            Id = 9,
            Name = "Joao Richa",
            Department = "Legacy Pod",
            AvatarUrl = "images/avatars/avatar4.png"
        };

        var pastedData = string.Join(Environment.NewLine, new[]
        {
            Row("Kissimmee", "", "Employee Relative Performance Dashboard Output", "", "", "March"),
            Row("1411", "", "", "", "", "4/3/2026"),
            Row("Employee", "Hours", "Total Rev", "TPH", "Rev Per Hour", "", "", "Apps / App Eff", "", "", "", "PMs / PM Eff", "", "", "", "Surveys / 5-Star", "", "", "", "Warranty Attach", "", "", "Dept Specific", "", "Rank", "KPIs Hit"),
            Row("Retail Programs-Center Store", "", "", "", "Actaul", "Goal", "Track", "", "Actaul", "Goal", "Track", "", "Actaul", "Goal", "Track", "", "Actaul", "Goal", "Track", "Actaul", "Goal", "Track", "PC Basket", "Office", "", ""),
            Row("Joao Aguiar de Pinho Pacheco", "142", "$104,832", "1.8", "$740", "$419", "On Track", "11", "$9,530", "$7,500", "Off Track", "7", "$11,775", "$6,500", "Off Track", "1", "5.0", "4.75", "On Track", "8.8%", "10%", "Off Track", "$68", "0.05", "2734", "2")
        });

        var result = _parser.Parse(pastedData, new[] { existingMember, staleMember });

        Assert.True(result.IsNewFormat);
        Assert.True(result.HasRows);
        Assert.Single(result.Pods);

        var pod = result.Pods.Single();
        var row = Assert.Single(pod.Rows);

        Assert.Equal(PodMemberMatchKind.Moved, row.Match.Kind);
        Assert.Equal(existingMember.Id, row.Match.Member?.Id);
        Assert.Equal("Retail Programs-Center Store", pod.SystemName);
        Assert.Equal("8.8", row.MetricValues["WarrantyAttach"]);
        Assert.Equal("68", row.MetricValues["PCBasket"]);
        Assert.Equal("0.05", row.MetricValues["Office"]);
    }

    [Fact]
    public void Match_DoesNotUseCrossPodFirstNameOnlyFallback()
    {
        var staleMember = new TeamMember
        {
            Id = 10,
            Name = "Joao Richa",
            Department = "Legacy Pod",
            AvatarUrl = "images/avatars/avatar4.png"
        };

        var match = PodMemberMatcher.Match(
            "Joao Aguiar de Pinho Pacheco",
            "Retail Programs-Center Store",
            Array.Empty<TeamMember>(),
            new[] { staleMember });

        Assert.Equal(PodMemberMatchKind.NewMember, match.Kind);
        Assert.Null(match.Member);
    }

    private static string Row(params string[] cells) => string.Join('\t', cells);
}
