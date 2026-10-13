using Contracts;

namespace StudentService.Student;

public interface IReadOnlyStudentHistory
{
    bool Attended(int day, Subject subject);
    bool? WasAsked(int day, Subject subject);
}
