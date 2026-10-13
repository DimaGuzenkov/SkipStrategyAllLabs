namespace HistoryService;

using System.Collections.Concurrent;
using HistoryService.Persistence;

public sealed class Service(IHistoryRepository repo, TimeSpan? timeout = null)
{
    private const int AllRecorded = 63;
    private readonly TimeSpan DefaultTimeout = timeout ?? TimeSpan.FromSeconds(30);

    private readonly IHistoryRepository _repo = repo;
    private readonly ConcurrentDictionary<(Guid, int), TaskCompletionSource> _waiters = new();

    public async Task RecordAsync(
        Guid sessionId, int day, int subject, bool asked, CancellationToken ct)
    {
        var newRecordedMask = await _repo.RecordAsync(sessionId, day, subject, asked, ct);

        if (newRecordedMask == AllRecorded)
        {
            if (_waiters.TryRemove((sessionId, day), out var tcs))
                tcs.TrySetResult();
        }
    }

    public async Task<byte> GetDayMaskAsync(Guid sessionId, int day, CancellationToken ct)
    {
        if (day == 0) return 0;
        var record = await _repo.GetDayRecordAsync(sessionId, day, ct);
        if (record is not null)
        {
            return record.Mask;
        }

        var tcs = _waiters.GetOrAdd((sessionId, day), _ => new TaskCompletionSource());

        try
        {
            await tcs.Task.WaitAsync(DefaultTimeout, ct);
        }
        catch (TimeoutException)
        {
            _waiters.TryRemove((sessionId, day), out _);
            throw new DayNotClosedException(sessionId, day);
        }

        var final = await _repo.GetDayRecordAsync(sessionId, day, ct);
        return final!.Mask;
    }

    public Task<IReadOnlyList<AskedRecordEntity>> GetAllClosedDaysAsync(
        Guid sessionId, CancellationToken ct)
        => _repo.GetAllClosedDaysAsync(sessionId, ct);
}

public sealed class DayNotClosedException(Guid sessionId, int day)
    : Exception($"Day {day} for session {sessionId} did not close within timeout")
{
    public Guid SessionId { get; } = sessionId;
    public int Day { get; } = day;
}
