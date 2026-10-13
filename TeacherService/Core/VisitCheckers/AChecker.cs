using TeacherService.Core.VisitCheckers;
namespace TeacherService.Core.VisitCheckers;

using Contracts;

public class AChecker(Subject A) : IVisitChecker
{
    private readonly Subject a = A;

    public bool IsChecking(int _, DaySnapshot prevQ)
    {
        return prevQ[a];
    }

    public override string ToString() => $"(A {a})";
}
