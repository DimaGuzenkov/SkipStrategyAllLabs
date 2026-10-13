namespace StudentService.Core;

using Contracts;

public sealed record SubjectContext(Subject Subject);

public interface ITeacher
{
    Task<bool> WasAskedAsync(Guid sessionId, int day, Subject subject, CancellationToken ct);
}
