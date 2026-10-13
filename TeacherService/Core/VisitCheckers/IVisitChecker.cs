using Contracts;
namespace TeacherService.Core.VisitCheckers;

public interface IVisitChecker
{
    bool IsChecking(int day, DaySnapshot prevQ);
}
