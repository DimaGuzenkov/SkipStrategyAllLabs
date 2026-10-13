namespace SessionService.Persistence;

using Microsoft.EntityFrameworkCore;

public interface ISessionRepository
{
    Task<SessionEntity?> GetActiveAsync(string studentId, CancellationToken ct);
    Task<SessionEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<SessionEntity> CreateAsync(string studentId, CancellationToken ct);
    Task UpdateProgressAsync(SessionEntity entity, CancellationToken ct);
}

public sealed class SessionRepository(SessionDbContext db) : ISessionRepository
{
    private readonly SessionDbContext _db = db;

    public Task<SessionEntity?> GetActiveAsync(string studentId, CancellationToken ct)
    {
        return _db.Sessions
            .AsNoTracking()
            .Where(s => s.StudentId == studentId && !s.Finished)
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public Task<SessionEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<SessionEntity> CreateAsync(string studentId, CancellationToken ct)
    {
        var session = new SessionEntity
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            SessionSeed = Random.Shared.NextInt64(),
            CurrentDay = 0,
            Utility = 0,
            Skips = 0,
            Expelled = false,
            Finished = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session;
    }

    public Task UpdateProgressAsync(SessionEntity entity, CancellationToken ct)
    {
        return _db.Sessions
            .Where(s => s.Id == entity.Id && s.CurrentDay <= entity.CurrentDay)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.CurrentDay, entity.CurrentDay)
                .SetProperty(x => x.Utility, entity.Utility)
                .SetProperty(x => x.Skips, entity.Skips)
                .SetProperty(x => x.Expelled, entity.Expelled)
                .SetProperty(x => x.Finished, entity.Finished)
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                ct);
    }
}
