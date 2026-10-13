using StudentService.Student;
using Contracts;

namespace StudentService.Core;

public sealed class SemesterOrchestrator(
    ITeacher teacher,
    ISkipStrategy strategy,
    IStudentHistory studentHistory)
{
    private readonly ITeacher _teacher = teacher;
    private readonly ISkipStrategy _strategy = strategy;
    private readonly IStudentHistory _studentHistory = studentHistory;
    private SemesterState? _state;

    private readonly IEnumerable<Subject> _subjects = Enum.GetValues<Subject>();

    public void SetHistory(SemesterState state, List<DaySnapshot> days)
    {
        _state = state;
        _studentHistory.Record(days);
    }

    public async Task<SemesterState> RunDayAsync(CancellationToken ct)
    {
        if (_state?.Finished is true) return _state;

        _state.NextDay();
        _state.AddPie();

        int day = _state.Day;

        var tasks = _subjects
            .Select(s => _teacher.WasAskedAsync(_state.SessionId, day, s, ct))
            .ToList();

        var attended = new DaySnapshot(_strategy.DecideDay(day, _studentHistory));
        _studentHistory.RecordAttended(day, attended);
        _state.AddSkip(6 - attended.Count);

        var asked = new DaySnapshot(await Task.WhenAll(tasks));
        _studentHistory.RecordAsked(day, asked);
        if (_studentHistory.IsExpelled(day)) _state.Expel();

        return _state;
    }
}
