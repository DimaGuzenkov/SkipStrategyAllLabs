namespace TeacherService.Tests.Fakes;

using Contracts;
using TeacherService.Core;


public sealed class FakeHistory : IHistory
{
    public DaySnapshot GetDayResponse { get; set; } = new(0);
    public List<(Guid SessionId, int Day)> GetDayCalls { get; } = [];
    public List<(Guid SessionId, int Day, Subject Subject, bool Asked)> RecordCalls { get; } = [];

    public Task<DaySnapshot> GetDayAsync(Guid sessionId, int day, CancellationToken ct)
    {
        GetDayCalls.Add((sessionId, day));
        return Task.FromResult(GetDayResponse);
    }

    public Task RecordAsync(Guid sessionId, int day, Subject subject, bool asked, CancellationToken ct)
    {
        RecordCalls.Add((sessionId, day, subject, asked));
        return Task.CompletedTask;
    }
}
