using Contracts;
namespace TeacherService.Core;

public interface IHistory
{
    Task<DaySnapshot> GetDayAsync(Guid sessionId, int day, CancellationToken ct);
    Task RecordAsync(Guid sessionId, int day, Subject subject, bool asked, CancellationToken ct);
}
