using Contracts;
using TeacherService.Persistence;
namespace TeacherService.Core;

public sealed record SubjectContext(Subject Subject);

public interface ITeacher
{
    Task<bool> WasAskedAsync(Guid sessionId, int day, CancellationToken ct);
}
