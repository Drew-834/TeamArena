using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Optional per-person pod metric targets (currently not mapped to the SQL schema)
        // These are ignored by EF Core on the server to avoid schema mismatches in production.
        [NotMapped]
        public double? RphGoal { get; set; }
        [NotMapped]
        public double? AppEffGoal { get; set; }
        [NotMapped]
        public double? PmEffGoal { get; set; }
        [NotMapped]
        public double? WarrantyAttachGoal { get; set; }
        [NotMapped]
        public double? AccAttachGoal { get; set; }

        public ICollection<MetricRecord> MetricRecords { get; set; } = new List<MetricRecord>();
    }
} 