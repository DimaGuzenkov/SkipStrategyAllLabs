using StudentService.Student;
namespace StudentService.Tests.Fakes;

using Contracts;
using StudentService.Core;

public sealed class FakeStudentHistory : IReadOnlyStudentHistory
{
    private readonly Dictionary<(int Day, Subject Subject), bool> _asked = [];

    public void Set(int day, Subject subject, bool asked) => _asked[(day, subject)] = asked;
    public void SetAll(int day, bool asked)
    {
        foreach (var s in Enum.GetValues<Subject>())
            _asked[(day, s)] = asked;
    }

    public bool Attended(int day, Subject subject) => false;
    public bool? WasAsked(int day, Subject subject)
        => _asked.TryGetValue((day, subject), out var v) && v;
}
