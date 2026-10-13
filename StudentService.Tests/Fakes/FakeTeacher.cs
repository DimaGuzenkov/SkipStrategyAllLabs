using StudentService.Core;
namespace StudentService.Tests.Fakes;

using Contracts;

public sealed class FakeTeacher(Func<Guid, int, Subject, bool> impl) : ITeacher
{
    private readonly Func<Guid, int, Subject, bool> _impl = impl;
    public List<(Guid SessionId, int Day, Subject Subject)> Calls { get; } = [];

    public Task<bool> WasAskedAsync(Guid sessionId, int day, Subject subject, CancellationToken ct)
    {
        Calls.Add((sessionId, day, subject));
        return Task.FromResult(_impl(sessionId, day, subject));
    }

    public static FakeTeacher Always(bool value) => new((_, _, _) => value);

    public static FakeTeacher OnEvenDays() => new((_, day, _) => day % 2 == 0);

    public static FakeTeacher OnlyFor(Subject subject) => new((_, _, s) => s == subject);
}
