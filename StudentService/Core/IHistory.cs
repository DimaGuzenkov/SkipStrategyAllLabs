using Contracts;

namespace StudentService.Core;

public interface IHistory
{
    Task<List<DaySnapshot>> GetAllDaysAsync(Guid sessionId, CancellationToken ct);
}
