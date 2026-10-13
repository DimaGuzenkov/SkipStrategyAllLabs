namespace TeacherService.Tests.VisitCheckers_Tests;

using NUnit.Framework;
using TeacherService.Core.VisitCheckers;


[TestFixture]
public class CounterRandomTests
{
    [Test]
    public void NextUInt64_SameInput_SameOutput()
    {
        var rng = new CounterRandom(42);

        var a = rng.NextUInt64(0);
        var b = rng.NextUInt64(0);

        Assert.That(a, Is.EqualTo(b), "Детерминированный RNG: одинаковый вход → одинаковый выход");
    }

    [Test]
    public void NextUInt64_DifferentIndex_DifferentOutput()
    {
        var rng = new CounterRandom(42);

        var a = rng.NextUInt64(0);
        var b = rng.NextUInt64(1);
        var c = rng.NextUInt64(2);

        Assert.That(new[] { a, b, c }, Is.Unique, "Разные индексы → разные значения (для малых индексов)");
    }

    [Test]
    public void NextUInt64_DifferentSeed_DifferentOutput()
    {
        var rng1 = new CounterRandom(42);
        var rng2 = new CounterRandom(43);

        Assert.That(rng1.NextUInt64(0), Is.Not.EqualTo(rng2.NextUInt64(0)));
    }

    [Test]
    public void NextInt_Max1_AlwaysZero()
    {
        var rng = new CounterRandom(42);

        for (ulong i = 0; i < 10; i++)
            Assert.That(rng.NextInt(i, 1), Is.EqualTo(0));
    }

    [Test]
    public void NextInt_RespectsRange()
    {
        var rng = new CounterRandom(42);

        for (ulong i = 0; i < 1000; i++)
        {
            var v = rng.NextInt(i, 6);
            Assert.That(v, Is.InRange(0, 5));
        }
    }

    [Test]
    public void NextInt_Max0_Throws()
    {
        var rng = new CounterRandom(42);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(0, 0));
    }

    [Test]
    public void NextInt_MaxNegative_Throws()
    {
        var rng = new CounterRandom(42);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(0, -1));
    }

    [Test]
    public void NextInt_DistributionRoughlyUniform()
    {
        var rng = new CounterRandom(42);
        var counts = new int[6];

        for (ulong i = 0; i < 6000; i++)
            counts[rng.NextInt(i, 6)]++;

        foreach (var c in counts)
            Assert.That(c, Is.InRange(700, 1300), $"Распределение не равномерное: {string.Join(",", counts)}");
    }
}
