namespace TeacherService.Tests.VisitCheckers_Tests;

using Contracts;
using NUnit.Framework;
using TeacherService.Core.VisitCheckers;


[TestFixture]
public class ACheckerTests
{
    [TestCase(Subject.Calculus)]
    [TestCase(Subject.LinearAlgebra)]
    [TestCase(Subject.DiscreteMath)]
    [TestCase(Subject.Programming)]
    [TestCase(Subject.Physics)]
    [TestCase(Subject.English)]
    public void IsChecking_ReturnsBitOfA(Subject a)
    {
        var checker = new AChecker(a);
        var withBit = new DaySnapshot().With(a, true);
        var without = new DaySnapshot(0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(checker.IsChecking(1, withBit), Is.True);
            Assert.That(checker.IsChecking(1, without), Is.False);
        }

    }

    [Test]
    public void IsChecking_IgnoresOtherBits()
    {
        // A = Physics (индекс 4). Все биты, кроме 4, установлены
        var checker = new AChecker(Subject.Physics);
        var onlyOthers = new DaySnapshot(0b101111);  // бит 4 = 0

        Assert.That(checker.IsChecking(1, onlyOthers), Is.False);
    }

    [Test]
    public void IsChecking_IgnoresDay()
    {
        var checker = new AChecker(Subject.Calculus);
        var snap = new DaySnapshot(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(checker.IsChecking(1, snap), Is.True);
            Assert.That(checker.IsChecking(100, snap), Is.True);
            Assert.That(checker.IsChecking(0, snap), Is.True);
        }

    }

    [Test]
    public void ToString_ContainsSubject()
    {
        var checker = new AChecker(Subject.Physics);
        Assert.That(checker.ToString(), Does.Contain("Physics"));
    }
}
