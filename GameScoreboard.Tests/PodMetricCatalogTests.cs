using GameScoreboard.Services.Tracking;

namespace GameScoreboard.Tests;

public class PodMetricCatalogTests
{
    [Fact]
    public void PublicDisplayMetrics_ExcludeDepartmentSpecificFields()
    {
        var visibleMetrics = PodMetricCatalog.GetPublicDisplayMetrics("Home Theater-HT/CA");

        Assert.Equal(new[] { "RPH", "AppEff", "PMEff", "Surveys", "WarrantyAttach" }, visibleMetrics);
        Assert.DoesNotContain("HTBasket", visibleMetrics);
        Assert.DoesNotContain("ServAttach", visibleMetrics);
    }

    [Fact]
    public void ApplicableMetrics_VaryByDepartmentType()
    {
        var htMetrics = PodMetricCatalog.GetApplicableMetrics("Home Theater-HT/CA");
        var pcMetrics = PodMetricCatalog.GetApplicableMetrics("Retail Programs-Center Store");

        Assert.Contains("HTBasket", htMetrics);
        Assert.Contains("ServAttach", htMetrics);
        Assert.DoesNotContain("PCBasket", htMetrics);

        Assert.Contains("PCBasket", pcMetrics);
        Assert.Contains("Office", pcMetrics);
        Assert.DoesNotContain("HTBasket", pcMetrics);
    }

    [Fact]
    public void MetricTargets_UseUpdatedDepartmentSpecificGoals()
    {
        Assert.Equal(300.0, PodMetricCatalog.GetTarget("HTBasket"));
        Assert.Equal(25.0, PodMetricCatalog.GetTarget("ServAttach"));
        Assert.Equal(140.0, PodMetricCatalog.GetTarget("PCBasket"));
        Assert.Equal(25.0, PodMetricCatalog.GetTarget("Office"));
    }
}
