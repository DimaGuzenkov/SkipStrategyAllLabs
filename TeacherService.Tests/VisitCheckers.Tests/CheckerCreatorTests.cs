using TeacherService.Persistence;
namespace TeacherService.Tests.VisitCheckers_Tests;

using Contracts;
using NUnit.Framework;
using TeacherService.Core.VisitCheckers;


[TestFixture]
public class CheckerCreatorTests
{
    private static CheckerCreator MakeCreator(ulong seed = 42)
        => new(new CounterRandom(seed));

    [Test]
    public void CreateCheckerConfig_SetsSessionId()
    {
        var creator = MakeCreator();
        var id = Guid.NewGuid();

        var config = creator.CreateCheckerConfig(id);

        Assert.That(config.SessionId, Is.EqualTo(id));
    }

    [Test]
    public void CreateCheckerConfig_RuleKindInRange()
    {
        var creator = MakeCreator();

        for (int i = 0; i < 100; i++)
        {
            var config = creator.CreateCheckerConfig(Guid.NewGuid());
            Assert.That(config.RuleKind, Is.InRange(0, 2));
        }
    }

    [Test]
    public void CreateCheckerConfig_Kind1_AInRange()
    {
        var creator = MakeCreator();
        var found = false;

        for (int i = 0; i < 200; i++)
        {
            var config = creator.CreateCheckerConfig(Guid.NewGuid());
            if (config.RuleKind != 1) continue;
            found = true;
            Assert.That(config.A, Is.InRange(0, 5));
        }

        Assert.That(found, Is.True, "Не сгенерировано ни одного RuleKind=1 за 200 попыток");
    }

    [Test]
    public void CreateCheckerConfig_Kind2_AandBInRange_BdiffersFromA()
    {
        var creator = MakeCreator();
        var found = false;

        for (int i = 0; i < 200; i++)
        {
            var config = creator.CreateCheckerConfig(Guid.NewGuid());
            if (config.RuleKind != 2) continue;

            found = true;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.A, Is.InRange(0, 5));
                Assert.That(config.B, Is.InRange(0, 5));
            }

            Assert.That(config.B, Is.Not.EqualTo(config.A), "B должен отличаться от A");
        }

        Assert.That(found, Is.True, "Не сгенерировано ни одного RuleKind=2 за 200 попыток");
    }

    [Test]
    public void CreateCheckerConfig_Deterministic_SameIdSameConfig()
    {
        var id = Guid.NewGuid();

        var c1 = MakeCreator().CreateCheckerConfig(id);
        var c2 = MakeCreator().CreateCheckerConfig(id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(c2.SessionId, Is.EqualTo(c1.SessionId));
            Assert.That(c2.RuleKind, Is.EqualTo(c1.RuleKind));
            Assert.That(c2.A, Is.EqualTo(c1.A));
            Assert.That(c2.B, Is.EqualTo(c1.B));
            Assert.That(c2.Seed, Is.EqualTo(c1.Seed));
        }
    }

    [Test]
    public void CreateByConfig_Kind0_ReturnsRandomChecker()
    {
        var config = new TeacherRuleEntity { RuleKind = 0, Seed = 42 };
        var checker = CheckerCreator.CreateByConfig(config);
        Assert.That(checker, Is.InstanceOf<RandomChecker>());
    }

    [Test]
    public void CreateByConfig_Kind1_ReturnsAChecker()
    {
        var config = new TeacherRuleEntity { RuleKind = 1, A = 3 };
        var checker = CheckerCreator.CreateByConfig(config);
        Assert.That(checker, Is.InstanceOf<AChecker>());
    }

    [Test]
    public void CreateByConfig_Kind2_ReturnsABChecker()
    {
        var config = new TeacherRuleEntity { RuleKind = 2, A = 1, B = 4 };
        var checker = CheckerCreator.CreateByConfig(config);
        Assert.That(checker, Is.InstanceOf<ABChecker>());
    }

    [Test]
    public void CreateByConfig_Kind1_UsesCorrectSubject()
    {
        // AChecker(Subject.LinearAlgebra) должен проверять бит 1
        var config = new TeacherRuleEntity { RuleKind = 1, A = 1 };
        var checker = CheckerCreator.CreateByConfig(config);

        var withBit1 = new DaySnapshot(0b000010);
        var without = new DaySnapshot(0b000000);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(checker.IsChecking(1, withBit1), Is.True);
            Assert.That(checker.IsChecking(1, without), Is.False);
        }

    }
}
