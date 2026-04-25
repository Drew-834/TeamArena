namespace GameScoreboard.Models;

public readonly record struct QuestSupplementLine(
    string MetricKey,
    bool OffTrack,
    string LabelPrefix,
    string GoalFormatted);
