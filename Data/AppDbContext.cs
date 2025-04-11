using Microsoft.EntityFrameworkCore;
using GameScoreboard.Models;

namespace GameScoreboard.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TeamMember> TeamMembers { get; set; } = null!;
        public DbSet<MetricRecord> MetricRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for MetricRecord
            modelBuilder.Entity<MetricRecord>()
                .HasKey(mr => new { mr.TeamMemberId, mr.Period, mr.MetricKey });

            // Configure relationship between TeamMember and MetricRecord
            modelBuilder.Entity<MetricRecord>()
                .HasOne(mr => mr.TeamMember)
                .WithMany(tm => tm.MetricRecords) // Add this navigation property to TeamMember
                .HasForeignKey(mr => mr.TeamMemberId);

            // Seed initial data (optional, but useful for testing)
            // We can add seeding later if needed, using the old MockData logic
            // modelBuilder.Entity<TeamMember>().HasData(...);
        }
    }

    // New entity for storing individual metric values per period
    public class MetricRecord
    {
        // Composite Key Part 1: Foreign Key to TeamMember
        public int TeamMemberId { get; set; }
        
        // Composite Key Part 2: The tracking period (e.g., "Mid-Jan 2024")
        public required string Period { get; set; } 

        // Composite Key Part 3: The specific metric identifier (e.g., "GSP", "Revenue")
        public required string MetricKey { get; set; }
        
        // The value of the metric for this member during this period
        // Storing as string to handle different types initially, could refine later
        public string? Value { get; set; }

        // Navigation Property back to the TeamMember
        public TeamMember TeamMember { get; set; } = null!;
    }
} 