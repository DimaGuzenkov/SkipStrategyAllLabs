using Microsoft.EntityFrameworkCore;

namespace HistoryService.Persistence;

public sealed class HistoryDbContext(DbContextOptions<HistoryDbContext> options) : DbContext(options)
{
    public DbSet<AskedRecordEntity> AskedRecords => Set<AskedRecordEntity>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<AskedRecordEntity>(e =>
        {
            e.ToTable("asked_records");
            e.HasKey(x => new { x.SessionId, x.Day });
            e.Property(x => x.SessionId).HasColumnName("session_id");
            e.Property(x => x.Day).HasColumnName("day");
            e.Property(x => x.Mask).HasColumnName("mask");
            e.Property(x => x.RecordedMask).HasColumnName("recorded_mask");
        });
    }
}