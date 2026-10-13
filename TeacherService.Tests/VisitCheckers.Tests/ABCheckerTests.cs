namespace TeacherService.Tests.VisitCheckers_Tests;

using Contracts;
using NUnit.Framework;
using TeacherService.Core.VisitCheckers;


[TestFixture]
public class ABCheckerTests
{
    [TestCase(0, 0, false)]  // 0 XOR 0 = 0
    [TestCase(1, 0, true)]   // 1 XOR 0 = 1
    [TestCase(0, 1, true)]   // 0 XOR 1 = 1
    [TestCase(1, 1, false)]  // 1 XOR 1 = 0
    public void IsChecking_TruthTable(int bitA, int bitB, bool expected)
    {
        var checker = new ABChecker(Subject.Calculus, Subject.English);

        byte mask = 0;
        if (bitA == 1) mask |= 1 << (int)Subject.Calculus;
        if (bitB == 1) mask |= 1 << (int)Subject.English;
        var snap = new DaySnapshot(mask);

        Assert.That(checker.IsChecking(1, snap), Is.EqualTo(expected));
    }

    [TestCase(Subject.Calculus, Subject.LinearAlgebra)]
    [TestCase(Subject.DiscreteMath, Subject.Physics)]
    [TestCase(Subject.Programming, Subject.English)]
    public void IsChecking_VariousPairs(Subject a, Subject b)
    {
        var checker = new ABChecker(a, b);

        byte bitA = (byte)(1 << (int)a);
        byte bitB = (byte)(1 << (int)b);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(checker.IsChecking(1, new DaySnapshot(0)), Is.False);
            Assert.That(checker.IsChecking(1, new DaySnapshot(bitA)), Is.True);
            Assert.That(checker.IsChecking(1, new DaySnapshot(bitB)), Is.True);
            Assert.That(checker.IsChecking(1, new DaySnapshot((byte)(bitA | bitB))), Is.False);
        }

    }

    [Test]
    public void IsChecking_IgnoresOtherBits()
    {
        var checker = new ABChecker(Subject.Calculus, Subject.English);
        // Установлены все биты, кроме 0 и 5
        var onlyOthers = new DaySnapshot(0b011110);

        Assert.That(checker.IsChecking(1, onlyOthers), Is.False);
    }

    [Test]
    public void IsChecking_IgnoresDay()
    {
        var checker = new ABChecker(Subject.Calculus, Subject.English);
        var snap = new DaySnapshot(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(checker.IsChecking(1, snap), Is.True);
            Assert.That(checker.IsChecking(100, snap), Is.True);
        }

    }

    [Test]
    public void ToString_ContainsBothSubjects()
    {
        var checker = new ABChecker(Subject.Physics, Subject.English);
        var s = checker.ToString();

        Assert.That(s, Does.Contain("Physics"));
        Assert.That(s, Does.Contain("English"));
    }
}
