using Contracts;
using StudentService.Student;

namespace StudentService.Core;

public interface IStudentHistory : IReadOnlyStudentHistory
{
    void Record(int day, Subject subject, bool attended, bool asked);
    void RecordAttended(int day, Subject subject, bool attended);
    void RecordAsked(int day, Subject subject, bool asked);
    void RecordAttended(int day, DaySnapshot attended);
    void RecordAsked(int day, DaySnapshot asked);
    void Record(List<DaySnapshot> days);
    bool IsExpelled(int day);
    int CurrentDay { get; }
}