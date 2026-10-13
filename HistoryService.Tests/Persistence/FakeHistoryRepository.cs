using HistoryService.Persistence;

namespace HistoryService.Tests.Fakes;

public sealed class FakeHistoryRepository : IHistoryRepository
{
    private const byte AllRecorded = 63;
    private readonly Dictionary<(Guid, int), (byte Mask, byte RecordedMask)> _records = [];

    public List<(Guid SessionId, int Day, int Subject, bool Asked)> RecordCalls { get; } = [];
    public List<(Guid SessionId, int Day)> GetDayCalls { get; } = [];

    /// <summary>Текущее recorded_mask для отладки и проверок.</summary>
    public byte GetRecordedMask(Guid sessionId, int day)
        => _records.TryGetValue((sessionId, day), out var rec) ? rec.RecordedMask : (byte)0;

    /// <summary>Текущее asked mask для отладки и проверок.</summary>
    public byte GetMask(Guid sessionId, int day)
        => _records.TryGetValue((sessionId, day), out var rec) ? rec.Mask : (byte)0;

    public void SetClosedDay(Guid sessionId, int day, byte mask)
        => _records[(sessionId, day)] = (mask, AllRecorded);

    public Task<AskedRecordEntity?> GetDayRecordAsync(Guid sessionId, int day, CancellationToken ct)
    {
        GetDayCalls.Add((sessionId, day));

        if (_records.TryGetValue((sessionId, day), out var rec) && rec.RecordedMask == AllRecorded)
        {
            return Task.FromResult<AskedRecordEntity?>(new AskedRecordEntity
            {
                SessionId = sessionId,
                Day = day,
                Mask = rec.Mask,
                RecordedMask = rec.RecordedMask
            });
        }
        return Task.FromResult<AskedRecordEntity?>(null);
    }

    public Task<byte> RecordAsync(Guid sessionId, int day, int subject, bool asked, CancellationToken ct)
    {
        RecordCalls.Add((sessionId, day, subject, asked));

        if (!_records.TryGetValue((sessionId, day), out var rec))
            rec = (0, 0);

        byte bit = (byte)(1 << subject);
        byte newMask = asked ? (byte)(rec.Mask | bit) : (byte)(rec.Mask & ~bit);
        byte newRecordedMask = (byte)(rec.RecordedMask | bit);
        _records[(sessionId, day)] = (newMask, newRecordedMask);

        return Task.FromResult(newRecordedMask);
    }

    public Task<IReadOnlyList<AskedRecordEntity>> GetAllClosedDaysAsync(Guid sessionId, CancellationToken ct)
    {
        var result = _records
            .Where(kv => kv.Key.Item1 == sessionId && kv.Value.RecordedMask == AllRecorded)
            .OrderBy(kv => kv.Key.Item2)
            .Select(kv => new AskedRecordEntity
            {
                SessionId = kv.Key.Item1,
                Day = kv.Key.Item2,
                Mask = kv.Value.Mask,
                RecordedMask = kv.Value.RecordedMask
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<AskedRecordEntity>>(result);
    }
}