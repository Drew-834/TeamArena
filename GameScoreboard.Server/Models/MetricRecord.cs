namespace GameScoreboard.Server.Models
{
    public class MetricRecord
    {
        public int TeamMemberId { get; set; }
        public required string Period { get; set; }
        public required string MetricKey { get; set; }
        public string? Value { get; set; }

        public TeamMember TeamMember { get; set; } = null!;
    }
} 