using System.Collections.Generic;

namespace GameScoreboard.Server.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Department { get; set; }
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
        public double TotalExperience { get; set; }

        public ICollection<MetricRecord> MetricRecords { get; set; } = new List<MetricRecord>();
    }
} 