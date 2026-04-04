using GameScoreboard.Server.Controllers;
using GameScoreboard.Server.Data;
using GameScoreboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameScoreboard.Tests;

public class MetricsControllerTests
{
    [Fact]
    public async Task Save_ReplacesExistingRecordsForMemberAndPeriod()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var db = new AppDbContext(options);
        db.TeamMembers.Add(new TeamMember
        {
            Id = 42,
            Name = "Daniel Datta",
            Department = "Retail Programs-Center Store"
        });
        db.MetricRecords.AddRange(
            new MetricRecord { TeamMemberId = 42, Period = "2026-04-03", MetricKey = "WarrantyAttach", Value = "15651" },
            new MetricRecord { TeamMemberId = 42, Period = "2026-04-03", MetricKey = "RPH", Value = "500" },
            new MetricRecord { TeamMemberId = 42, Period = "2026-03-20", MetricKey = "WarrantyAttach", Value = "7.0" });
        await db.SaveChangesAsync();

        var controller = new MetricsController(db);
        var newRecords = new List<MetricRecord>
        {
            new() { MetricKey = "WarrantyAttach", Period = "ignored", Value = "13.2" },
            new() { MetricKey = "RPH", Period = "ignored", Value = "545" }
        };

        var response = await controller.Save(42, "2026-04-03", newRecords);

        Assert.IsType<NoContentResult>(response);

        var currentPeriodRecords = await db.MetricRecords
            .Where(r => r.TeamMemberId == 42 && r.Period == "2026-04-03")
            .OrderBy(r => r.MetricKey)
            .ToListAsync();

        Assert.Collection(
            currentPeriodRecords,
            record =>
            {
                Assert.Equal("RPH", record.MetricKey);
                Assert.Equal("545", record.Value);
            },
            record =>
            {
                Assert.Equal("WarrantyAttach", record.MetricKey);
                Assert.Equal("13.2", record.Value);
            });

        var priorPeriodRecord = await db.MetricRecords.SingleAsync(r =>
            r.TeamMemberId == 42 &&
            r.Period == "2026-03-20" &&
            r.MetricKey == "WarrantyAttach");

        Assert.Equal("7.0", priorPeriodRecord.Value);
    }
}
