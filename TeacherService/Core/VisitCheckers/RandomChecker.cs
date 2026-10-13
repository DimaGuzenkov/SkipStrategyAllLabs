using Contracts;
namespace TeacherService.Core.VisitCheckers;

public class RandomChecker(ICounterRandom rnd) : IVisitChecker
{
    private readonly ICounterRandom _rnd = rnd;

    public bool IsChecking(int day, DaySnapshot _)
    {
        return _rnd.NextInt((ulong)day, 2) == 1;
    }

    public override string ToString() => "(Random)";
}
