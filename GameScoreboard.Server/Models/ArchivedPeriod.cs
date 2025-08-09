using System;

namespace GameScoreboard.Server.Models
{
    public class ArchivedPeriod
    {
        public int Id { get; set; }
        public required string Department { get; set; }
        public required string Period { get; set; }
        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
        public string? ArchivedByUserId { get; set; }
    }
} 