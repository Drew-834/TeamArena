using Microsoft.EntityFrameworkCore;
using GameScoreboard.Server.Models;

namespace GameScoreboard.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<MetricRecord> MetricRecords => Set<MetricRecord>();
        public DbSet<ArchivedPeriod> ArchivedPeriods => Set<ArchivedPeriod>();
        public DbSet<PodSnapshot> PodSnapshots => Set<PodSnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MetricRecord>()
                .HasKey(mr => new { mr.TeamMemberId, mr.Period, mr.MetricKey });

            modelBuilder.Entity<MetricRecord>()
                .HasOne(mr => mr.TeamMember)
                .WithMany(tm => tm.MetricRecords)
                .HasForeignKey(mr => mr.TeamMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamMember>()
                .Property(t => t.Name)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<TeamMember>()
                .Property(t => t.Department)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<ArchivedPeriod>()
                .HasIndex(a => new { a.Department, a.Period })
                .IsUnique();

            modelBuilder.Entity<PodSnapshot>()
                .Property(s => s.PodName).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<PodSnapshot>()
                .Property(s => s.JsonData).IsRequired();
            modelBuilder.Entity<PodSnapshot>()
                .HasIndex(s => new { s.PodName, s.SnapshotDate });
        }
    }
} 