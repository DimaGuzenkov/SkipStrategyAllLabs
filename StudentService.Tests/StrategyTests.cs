namespace StudentService.Tests;

using Contracts;
using NUnit.Framework;
using StudentService.Student;
using StudentService.Tests.Fakes;

[TestFixture]
public class StrategyTests
{
    private const int SubjectCount = 6;

    private static Strategy NewStrategy() => new();

    // ================= День 1: пустая история =================

    [Test]
    public void DecideDay_Day1EmptyHistory_AllTrue()
    {
        var strategy = NewStrategy();
        var history = new FakeStudentHistory();

        var decisions = strategy.DecideDay(1, history);

        Assert.That(decisions, Has.Length.EqualTo(SubjectCount));
        Assert.That(decisions, Is.All.True,
            "На пустой истории нет уверенности — идём на всё");
    }

    // ================= Несколько нулей — стратегия не уверена =================

    [Test]
    public void DecideDay_ThreeZeroDays_AllTrue()
    {
        var history = new FakeStudentHistory();
        for (int d = 1; d <= 3; d++) history.SetAll(d, asked: false);

        var decisions = NewStrategy().DecideDay(4, history);

        Assert.That(decisions, Is.All.True);
    }

    // ================= Много нулей — стратегия прогуливает =================

    [Test]
    public void DecideDay_FifteenZeroDays_AllFalse()
    {
        var history = new FakeStudentHistory();
        for (int d = 1; d <= 15; d++) history.SetAll(d, asked: false);

        var decisions = NewStrategy().DecideDay(16, history);

        Assert.That(decisions, Is.All.False,
            "После 15 нулевых дней детерминированные гипотезы уверенно предсказывают 0");
    }

    [Test]
    public void DecideDay_ThresholdAroundDay7()
    {
        for (int n = 3; n <= 15; n++)
        {
            var history = new FakeStudentHistory();
            for (int d = 1; d <= n; d++) history.SetAll(d, asked: false);

            var decisions = NewStrategy().DecideDay(n + 1, history);

            if (n <= 5)
                Assert.That(decisions, Is.All.True, $"День {n + 1}: рано прогуливать");
            else if (n >= 10)
                Assert.That(decisions, Is.All.False, $"День {n + 1}: пора прогуливать");
        }
    }

    // ================= Случайная история =================

    [Test]
    public void DecideDay_RandomHistory_AllTrue()
    {
        var rng = new Random(42);
        var history = new FakeStudentHistory();
        for (int d = 1; d <= 30; d++)
        {
            foreach (var s in Enum.GetValues<Subject>())
            {
                history.Set(d, s, rng.Next(2) == 1);
            }
        }

        var decisions = NewStrategy().DecideDay(31, history);

        Assert.That(decisions, Is.All.True,
            "На случайной истории детерминированные гипотезы вымирают, " +
            "остаётся Random → прогуливать нельзя");
    }

    // ================= Независимость предметов =================

    [Test]
    public void DecideDay_SubjectIndependence()
    {
        const int N = 20;
        var history = new FakeStudentHistory();
        for (int d = 1; d <= N; d++)
        {
            history.Set(d, Subject.Calculus, asked: false);
            history.Set(d, Subject.LinearAlgebra, asked: true);
            history.Set(d, Subject.DiscreteMath, asked: false);
            history.Set(d, Subject.Programming, asked: false);
            history.Set(d, Subject.Physics, asked: false);
            history.Set(d, Subject.English, asked: false);
        }

        var decisions = NewStrategy().DecideDay(N + 1, history);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(decisions[(int)Subject.Calculus], Is.False, "20 нулей → прогуливаем");
            Assert.That(decisions[(int)Subject.LinearAlgebra], Is.True, "20 единиц → идём");
            Assert.That(decisions[(int)Subject.DiscreteMath], Is.False);
            Assert.That(decisions[(int)Subject.Programming], Is.False);
            Assert.That(decisions[(int)Subject.Physics], Is.False);
            Assert.That(decisions[(int)Subject.English], Is.False);
        }

    }

    // ================= Идемпотентность =================

    [Test]
    public void DecideDay_SameInputTwice_SameOutput()
    {
        var history = new FakeStudentHistory();
        for (int d = 1; d <= 10; d++) history.SetAll(d, false);

        var strategy = NewStrategy();
        var first = strategy.DecideDay(11, history);
        var second = strategy.DecideDay(11, history);

        Assert.That(second, Is.EqualTo(first));
    }

    // ================= Догон истории =================

    [Test]
    public void DecideDay_CatchesUpFromDay1()
    {
        var history = new FakeStudentHistory();
        for (int d = 1; d <= 20; d++) history.SetAll(d, false);

        var strategy = NewStrategy();
        strategy.DecideDay(1, history);
        var decisions = strategy.DecideDay(21, history);

        Assert.That(decisions, Is.All.False,
            "Стратегия должна догнать историю между днями 1 и 21");
    }

    [Test]
    public void DecideDay_FreshInstanceAtDay20_Works()
    {
        var history = new FakeStudentHistory();
        for (int d = 1; d <= 20; d++) history.SetAll(d, false);

        // Свежий экземпляр, первый вызов сразу на 20-й день
        var strategy = NewStrategy();
        var decisions = strategy.DecideDay(20, history);

        Assert.That(decisions, Is.All.False,
            "Свежая стратегия должна восстановить состояние с нуля");
    }

    // ================= Reset на новом семестре =================

    [Test]
    public void DecideDay_Day1_ResetsState()
    {
        var strategy = NewStrategy();

        // Первый семестр: 15 нулей
        var h1 = new FakeStudentHistory();
        for (int d = 1; d <= 15; d++) h1.SetAll(d, false);
        strategy.DecideDay(1, h1);
        strategy.DecideDay(16, h1);

        // Второй семестр: снова с 1-го дня, пустая история
        var h2 = new FakeStudentHistory();
        var decisions = strategy.DecideDay(1, h2);

        Assert.That(decisions, Is.All.True,
            "Новый семестр должен сбросить состояние");
    }
}
