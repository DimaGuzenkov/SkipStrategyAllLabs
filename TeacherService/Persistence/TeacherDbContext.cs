namespace TeacherService.Persistence;

using Microsoft.EntityFrameworkCore;

public sealed class TeacherDbContext(DbContextOptions<TeacherDbContext> options) : DbContext(options)
{
    public DbSet<TeacherRuleEntity> Rules => Set<TeacherRuleEntity>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<TeacherRuleEntity>(e =>
        {
            e.ToTable("teacher_rules");
            e.HasKey(x => x.SessionId);
            e.Property(x => x.SessionId).HasColumnName("session_id");
            e.Property(x => x.RuleKind).HasColumnName("rule_kind");
            e.Property(x => x.A).HasColumnName("a");
            e.Property(x => x.B).HasColumnName("b");
            e.Property(x => x.Seed).HasColumnName("seed");
        });
    }
}
