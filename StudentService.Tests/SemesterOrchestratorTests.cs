namespace StudentService.Tests;

using Contracts;
using NUnit.Framework;
using StudentService.Core;
using StudentService.Student;
using StudentService.Tests.Fakes;

[TestFixture]
public class SemesterOrchestratorTests
{
    private static readonly Guid Session = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private SemesterState _state = null!;
    private StudentHistory _history = null!;

    private static SemesterOrchestrator Build(ITeacher teacher, ISkipStrategy strategy, IStudentHistory history)
        => new(teacher, strategy, history);

    [SetUp]
    public void Setup()
    {
        _state = new SemesterState();
        _history = new StudentHistory();
    }

    private SemesterOrchestrator BuildWith(ITeacher teacher, ISkipStrategy strategy)
        => Build(teacher, strategy, _history);

    // ================= Инициализация =================

    [Test]
    public void SetHistory_SeedsDays()
    {
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.AllAttend();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 3, 5, 0, false, false));
        orch.SetHistory(_state,
        [
            new(0b000001),
            new(0b000010),
            new(0b000100),
        ]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_history.WasAsked(1, Subject.Calculus), Is.True);
            Assert.That(_history.WasAsked(2, Subject.LinearAlgebra), Is.True);
            Assert.That(_history.WasAsked(3, Subject.DiscreteMath), Is.True);
        }

    }

    // ================= RunDay: happy path =================

    [Test]
    public async Task RunDay_AllAttend_NoneAsked_UtilityPlusOne()
    {
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.AllAttend();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        var result = await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Day, Is.EqualTo(1));
            Assert.That(result.Utility, Is.EqualTo(1), "+1 за пирожок");
            Assert.That(result.Skips, Is.EqualTo(0));
            Assert.That(result.Expelled, Is.False);
        }

    }

    [Test]
    public async Task RunDay_AllSkip_NoneAsked_UtilityPlusSeven()
    {
        // +1 пирожок + 6 пропущенных пар (не спросили)
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.AllSkip();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        var result = await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Utility, Is.EqualTo(7));
            Assert.That(result.Skips, Is.EqualTo(6));
            Assert.That(result.Expelled, Is.False);
        }

    }

    [Test]
    public async Task RunDay_AllSkip_AllAsked_Expelled()
    {
        var teacher = FakeTeacher.Always(true);
        var strategy = FakeStrategy.AllSkip();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        var result = await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Expelled, Is.True);
            Assert.That(result.Utility, Is.EqualTo(0), "Utility обнулена при вылете");
            Assert.That(result.Finished, Is.True);
        }

    }

    [Test]
    public async Task RunDay_OnlyFirstAttend_OthersSkipped_Five_NotAsked()
    {
        // Стратегия: идём только на Calculus.
        // Учителя: никто не спрашивает.
        // Результат: +1 пирожок + 5 пропущенных пар = 6
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.OnlyFirst();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        var result = await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Utility, Is.EqualTo(6));
            Assert.That(result.Skips, Is.EqualTo(5));
        }

    }

    // ================= Все учителя вызваны =================

    [Test]
    public async Task RunDay_AllSixTeachersCalled()
    {
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.AllAttend();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        await orch.RunDayAsync(CancellationToken.None);

        Assert.That(teacher.Calls, Has.Count.EqualTo(6));
        var subjects = teacher.Calls.Select(c => c.Subject).ToHashSet();
        Assert.That(subjects, Is.EquivalentTo(Enum.GetValues<Subject>()));
    }

    // ================= Callback в историю =================

    [Test]
    public async Task RunDay_RecordsAttendedInHistory()
    {
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.OnlyFirst();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_history.Attended(1, Subject.Calculus), Is.True);
            Assert.That(_history.Attended(1, Subject.Physics), Is.False);
        }

    }

    [Test]
    public async Task RunDay_RecordsAskedInHistory()
    {
        var teacher = FakeTeacher.OnlyFor(Subject.Physics);
        var strategy = FakeStrategy.AllSkip();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_history.WasAsked(1, Subject.Physics), Is.True);
            Assert.That(_history.WasAsked(1, Subject.Calculus), Is.False);
        }

    }

    // ================= Вылет при частичном прогуле =================

    [Test]
    public async Task RunDay_SkippedPhysicsAsked_Expelled()
    {
        // Идём только на Calculus, Physics спрашивают
        var teacher = FakeTeacher.OnlyFor(Subject.Physics);
        var strategy = FakeStrategy.OnlyFirst();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orch.SetHistory(_state, []);

        var result = await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Expelled, Is.True);
            Assert.That(result.Finished, Is.True);
        }

    }

    // ================= Завершение через 100 дней =================

    [Test]
    public async Task RunDay_AfterHundredDays_Finished()
    {
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.AllAttend();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 99, 100, 0, false, false));
        orch.SetHistory(_state, []);

        var result = await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Day, Is.EqualTo(100));
            Assert.That(result.Finished, Is.True);
        }

    }

    // ================= Идемпотентность при Finished =================

    [Test]
    public async Task RunDay_AlreadyFinished_ReturnsSameState()
    {
        var teacher = FakeTeacher.Always(false);
        var strategy = FakeStrategy.AllAttend();
        var orch = BuildWith(teacher, strategy);

        _state.Restore(new SessionDto(Session, "s", 1, 100, 100, 0, false, true));
        orch.SetHistory(_state, []);

        var result = await orch.RunDayAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Finished, Is.True);
            Assert.That(result.Day, Is.EqualTo(100), "День не должен двигаться");
            Assert.That(teacher.Calls, Is.Empty, "Учителя не должны вызываться");
        }

    }
}
