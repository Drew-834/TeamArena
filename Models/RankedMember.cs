using System;

namespace GameScoreboard.Models
{
    /// <summary>
    /// Represents a team member with their calculated rank and score for a specific period or context.
    /// Used primarily for displaying ranked lists.
    /// </summary>
    public class RankedMember
    {
        public int Id { get; set; }         // Original TeamMember ID
        public required string Name { get; set; }
        public required string Department { get; set; }
        public double Score { get; set; }   // The score used for ranking (e.g., TotalExperience)
        public int Rank { get; set; }       // The calculated rank (1st, 2nd, etc.)
    }
} 