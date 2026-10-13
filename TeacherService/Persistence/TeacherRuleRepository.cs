namespace TeacherService.Persistence;

using Microsoft.EntityFrameworkCore;

public interface ITeacherRuleRepository
{
    Task SaveAsync(Guid sessionId, TeacherRuleEntity config, CancellationToken ct);
    Task<TeacherRuleEntity?> LoadAsync(Guid sessionId, CancellationToken ct);
}

public sealed class TeacherRuleRepository(TeacherDbContext db) : ITeacherRuleRepository
{
    private readonly TeacherDbContext _db = db;

    public async Task SaveAsync(Guid sessionId, TeacherRuleEntity config, CancellationToken ct)
    {
        _db.Rules.Add(config);
        await _db.SaveChangesAsync(ct);
    }

    public Task<TeacherRuleEntity?> LoadAsync(Guid sessionId, CancellationToken ct)
    {
        return _db.Rules.AsNoTracking()
                  .FirstOrDefaultAsync(t => t.SessionId == sessionId, ct);
    }
}
