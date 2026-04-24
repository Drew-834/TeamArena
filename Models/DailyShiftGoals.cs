namespace GameScoreboard.Models;

public record DailyShiftGoals(
    double RphGoal, bool RphBuffed, int RphBuffPercent,
    double? AppGoal, bool AppBuffed, int AppBuffPercent,
    double? PmGoal, bool PmBuffed, int PmBuffPercent,
    double? WarrantyAttachGoalPercent, bool WarrantyBuffed, int WarrantyBuffPercent)
{
    public bool AnyBuffed => RphBuffed || AppBuffed || PmBuffed || WarrantyBuffed;
}
