using TeacherService.Core.VisitCheckers;
namespace TeacherService.Core.VisitCheckers;

using Contracts;

public class ABChecker(Subject A, Subject B) : IVisitChecker
{
    private readonly Subject a = A;
    private readonly Subject b = B;

    public bool IsChecking(int _, DaySnapshot prevQ)
    {
        return prevQ[a] ^ prevQ[b];
    }

    public override string ToString() => $"(A {a}, B {b})";
}
