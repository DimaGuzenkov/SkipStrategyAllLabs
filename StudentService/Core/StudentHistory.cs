using Contracts;

namespace StudentService.Core;

public sealed class StudentHistory : IStudentHistory
{
    private readonly DaySnapshot[] _attended = new DaySnapshot[101];
    private readonly DaySnapshot[] _asked = new DaySnapshot[101];

    public int CurrentDay { get; private set; }

    public bool Attended(int day, Subject subject)
    {
        return _attended[day][subject];
    }

    public bool? WasAsked(int day, Subject subject)
    {
        return _asked[day][subject];
    }

    public void Record(int day, Subject subject, bool attended, bool asked)
    {
        _attended[day] = _attended[day].With(subject, attended);
        _asked[day] = _asked[day].With(subject, asked);
        CurrentDay = Math.Max(CurrentDay, day);
    }

    public void Record(List<DaySnapshot> days)
    {
        var count = days.Count;
        CurrentDay = count;
        for (var i = 0; i < count; ++i)
        {
            _asked[i + 1] = days[i];
        }
    }

    public void RecordAttended(int day, Subject subject, bool attended)
    {
        _attended[day] = _attended[day].With(subject, attended);
        CurrentDay = Math.Max(CurrentDay, day);
    }

    public void RecordAsked(int day, Subject subject, bool asked)
    {
        _asked[day] = _asked[day].With(subject, asked);
        CurrentDay = Math.Max(CurrentDay, day);
    }

    public void RecordAttended(int day, DaySnapshot attended)
    {
        _attended[day] = attended;
    }

    public void RecordAsked(int day, DaySnapshot asked)
    {
        _asked[day] = asked;
    }

    public bool IsExpelled(int day)
    {
        var asked = _asked[day].Mask;
        var attended = _attended[day].Mask;
        return (attended - asked) != (attended ^ asked);
    }
}
