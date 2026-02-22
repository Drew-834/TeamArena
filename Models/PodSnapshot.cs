namespace GameScoreboard.Models
{
    public class PodSnapshot
    {
        public int Id { get; set; }
        public required string PodName { get; set; }
        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;
        public string? Label { get; set; }
        public required string JsonData { get; set; }
    }
}
