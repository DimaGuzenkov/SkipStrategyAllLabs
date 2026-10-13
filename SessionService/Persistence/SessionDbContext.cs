namespace SessionService.Persistence;

using Microsoft.EntityFrameworkCore;

public sealed class SessionDbContext(DbContextOptions<SessionDbContext> options) : DbContext(options)
{
    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<SessionEntity>(e =>
        {
            e.ToTable("sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.StudentId).HasColumnName("student_id").IsRequired();
            e.Property(x => x.SessionSeed).HasColumnName("session_seed");
            e.Property(x => x.CurrentDay).HasColumnName("current_day");
            e.Property(x => x.Utility).HasColumnName("utility");
            e.Property(x => x.Skips).HasColumnName("skips");
            e.Property(x => x.Expelled).HasColumnName("expelled");
            e.Property(x => x.Finished).HasColumnName("finished");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(x => x.StudentId)
             .HasFilter("\"finished\" = false")
             .HasDatabaseName("idx_sessions_active");
        });
    }
}
