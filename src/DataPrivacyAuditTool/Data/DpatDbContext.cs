using Microsoft.EntityFrameworkCore;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Data
{
    public class DpatDbContext : DbContext
    {
        public DpatDbContext(DbContextOptions<DpatDbContext> options) : base(options)
        {
        }

        public DbSet<AuditHistory> AuditHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.OverallScore)
                    .HasColumnType("REAL");

                entity.Property(e => e.AuditDate)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();
            });
        }
    }
}
