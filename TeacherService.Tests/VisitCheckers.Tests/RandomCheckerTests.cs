namespace TeacherService.Tests.VisitCheckers_Tests;

using Contracts;
using NUnit.Framework;
using TeacherService.Core.VisitCheckers;
using TeacherService.Tests.Fakes;

[TestFixture]
public class RandomCheckerTests
{
    [Test]
    public void IsChecking_ReturnsFalse_WhenRngReturns0()
    {
        var rng = StubCounterRandom.Fixed(0);
        var checker = new RandomChecker(rng);

        Assert.That(checker.IsChecking(1, new DaySnapshot(0)), Is.False);
    }

    [Test]
    public void IsChecking_ReturnsTrue_WhenRngReturns1()
    {
        var rng = StubCounterRandom.Fixed(1);
        var checker = new RandomChecker(rng);

        Assert.That(checker.IsChecking(1, new DaySnapshot(0)), Is.True);
    }

    [Test]
    public void IsChecking_UsesDayAsIndex()
    {
        var (stub, calls) = StubCounterRandom.Tracking((_, _) => 0);
        var checker = new RandomChecker(stub);

        checker.IsChecking(42, new DaySnapshot(0));

        Assert.That(calls, Has.Count.EqualTo(1));
        Assert.That(calls[0], Is.EqualTo(42), "RandomChecker должен передавать day как index");
    }

    [Test]
    public void IsChecking_UsesMax2()
    {
        int lastMax = -1;
        var rng = new StubCounterRandom((_, max) => { lastMax = max; return 0; });
        var checker = new RandomChecker(rng);

        checker.IsChecking(1, new DaySnapshot(0));

        Assert.That(lastMax, Is.EqualTo(2), "RandomChecker должен звать NextInt(index, 2)");
    }

    [Test]
    public void IsChecking_IgnoresSnapshot()
    {
        var rng = StubCounterRandom.Fixed(1);
        var checker = new RandomChecker(rng);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(checker.IsChecking(1, new DaySnapshot(0)), Is.True);
            Assert.That(checker.IsChecking(1, new DaySnapshot(0b111111)), Is.True);
        }

    }

    [Test]
    public void IsChecking_Deterministic_WithRealCounterRandom()
    {
        // Реальный CounterRandom с фиксированным seed — детерминирован
        var rng = new CounterRandom(seed: 42);
        var checker = new RandomChecker(rng);

        var first = checker.IsChecking(5, new DaySnapshot(0));
        var second = checker.IsChecking(5, new DaySnapshot(0));

        Assert.That(second, Is.EqualTo(first), "С одним seed и днём результат должен повторяться");
    }

    [Test]
    public void IsChecking_DifferentDays_CanDiffer()
    {
        var rng = new CounterRandom(seed: 42);
        var checker = new RandomChecker(rng);

        // Собираем результаты за 100 дней
        var results = new HashSet<bool>();
        for (int day = 1; day <= 100; day++)
            results.Add(checker.IsChecking(day, new DaySnapshot(0)));

        Assert.That(results, Has.Count.EqualTo(2),
            "За 100 дней должны встретиться и true, и false");
    }

    [Test]
    public void IsChecking_DistributionRoughlyBalanced()
    {
        var rng = new CounterRandom(seed: 42);
        var checker = new RandomChecker(rng);
        int trues = 0;

        for (int day = 1; day <= 1000; day++)
            if (checker.IsChecking(day, new DaySnapshot(0))) trues++;

        Assert.That(trues, Is.InRange(400, 600),
            $"Ожидалось ~50% true, получено {trues}/1000");
    }

    [Test]
    public void ToString_ReturnsRandomMarker()
    {
        var rng = StubCounterRandom.Fixed(0);
        var checker = new RandomChecker(rng);

        Assert.That(checker.ToString(), Is.EqualTo("(Random)"));
    }
}
