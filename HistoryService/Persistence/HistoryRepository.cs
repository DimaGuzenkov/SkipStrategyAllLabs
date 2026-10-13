namespace HistoryService.Persistence;

using Microsoft.EntityFrameworkCore;

public interface IHistoryRepository
{
    Task<AskedRecordEntity?> GetDayRecordAsync(Guid sessionId, int day, CancellationToken ct);
    Task<IReadOnlyList<AskedRecordEntity>> GetAllClosedDaysAsync(Guid sessionId, CancellationToken ct);
    Task<byte> RecordAsync(Guid sessionId, int day, int subject, bool asked, CancellationToken ct);
}

public sealed class HistoryRepository(HistoryDbContext db) : IHistoryRepository
{
    private readonly HistoryDbContext _db = db;
    private const int AllRecorded = 63;

    public Task<AskedRecordEntity?> GetDayRecordAsync(Guid sessionId, int day, CancellationToken ct)
    {
        return _db.AskedRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SessionId == sessionId && r.Day == day && r.RecordedMask == AllRecorded, ct);
    }

    public async Task<IReadOnlyList<AskedRecordEntity>> GetAllClosedDaysAsync(Guid sessionId, CancellationToken ct)
    {
        return await _db.AskedRecords
            .AsNoTracking()
            .Where(r => r.SessionId == sessionId && r.RecordedMask == AllRecorded)
            .OrderBy(r => r.Day)
            .ToListAsync(ct);
    }

    public async Task<byte> RecordAsync(
        Guid sessionId, int day, int subject, bool asked, CancellationToken ct)
    {
        byte askedBit = (byte)(1 << subject);
        byte askedValue = asked ? askedBit : (byte)0;

        var results = await _db.Database
            .SqlQueryRaw<byte>(@"
                INSERT INTO asked_records (session_id, day, mask, recorded_mask)
                VALUES ({0}, {1}, {2}, {3})
                ON CONFLICT (session_id, day) DO UPDATE
                SET mask = (asked_records.mask & ~{4}) | {5},
                    recorded_mask = asked_records.recorded_mask | {6}
                RETURNING recorded_mask",
                [
                    sessionId, day, askedValue, askedBit,
                    askedBit, askedValue, askedBit
                ])
            .ToArrayAsync(ct);

        if (results.Length == 0)
        {
            throw new InvalidOperationException(
                $"UPSERT for session={sessionId} day={day} subject={subject} returned no rows");
        }


        return results[0];
    }
}