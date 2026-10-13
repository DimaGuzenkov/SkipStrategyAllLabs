namespace StudentService.Tests.Fakes;

using StudentService.Student;
using StudentService.Core;
using Contracts;

public sealed class FakeStrategy(Func<int, IReadOnlyStudentHistory, bool[]> impl) : ISkipStrategy
{
    private readonly Func<int, IReadOnlyStudentHistory, bool[]> _impl = impl;

    public string Name => "Fake";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history) => _impl(day, history);

    public static FakeStrategy AllAttend() => new((_, _) => [true, true, true, true, true, true]);

    public static FakeStrategy AllSkip() => new((_, _) => [false, false, false, false, false, false]);

    public static FakeStrategy OnlyFirst() => new((_, _) => [true, false, false, false, false, false]);
}
