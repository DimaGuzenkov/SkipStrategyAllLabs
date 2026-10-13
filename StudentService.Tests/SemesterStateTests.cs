namespace StudentService.Tests;

using Contracts;
using NUnit.Framework;
using StudentService.Core;

[TestFixture]
public class SemesterStateTests
{
    private SemesterState _state = null!;

    [SetUp]
    public void Setup() => _state = new SemesterState();

    // ================= Начальное состояние =================

    [Test]
    public void New_AllZero()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.Day, Is.EqualTo(0));
            Assert.That(_state.Utility, Is.EqualTo(0));
            Assert.That(_state.Skips, Is.EqualTo(0));
            Assert.That(_state.Expelled, Is.False);
            Assert.That(_state.NotAsked, Is.EqualTo(0));
            Assert.That(_state.SessionId, Is.EqualTo(Guid.Empty));
        }
    }

    [Test]
    public void New_NotFinished()
    {
        Assert.That(_state.Finished, Is.False);
    }

    // ================= NextDay =================

    [Test]
    public void NextDay_Increments()
    {
        _state.NextDay();
        Assert.That(_state.Day, Is.EqualTo(1));

        _state.NextDay();
        Assert.That(_state.Day, Is.EqualTo(2));
    }

    // ================= Utility =================

    [Test]
    public void AddPie_IncrementsUtilityOnly()
    {
        _state.AddPie();
        _state.AddPie();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.Utility, Is.EqualTo(2));
            Assert.That(_state.Skips, Is.EqualTo(0));
        }

    }

    [Test]
    public void AddSkip_IncrementsUtilityAndSkips()
    {
        _state.AddSkip();
        _state.AddSkip();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.Utility, Is.EqualTo(2));
            Assert.That(_state.Skips, Is.EqualTo(2));
        }

    }

    [Test]
    public void AddSkip_WithCount_IncrementsByCount()
    {
        _state.AddSkip(4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.Utility, Is.EqualTo(4));
            Assert.That(_state.Skips, Is.EqualTo(4));
        }

    }

    // ================= Expel =================

    [Test]
    public void Expel_SetsFlagAndResetsUtility()
    {
        _state.AddPie();
        _state.AddPie();
        _state.AddSkip(3);

        _state.Expel();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.Expelled, Is.True);
            Assert.That(_state.Utility, Is.EqualTo(0));
            Assert.That(_state.Skips, Is.EqualTo(3), "Skips не сбрасывается");
        }

    }

    [Test]
    public void Expel_MakesFinished()
    {
        _state.Expel();
        Assert.That(_state.Finished, Is.True);
    }

    // ================= NotAsked =================

    [Test]
    public void AddNotAsked_Increments()
    {
        _state.AddNotAsked();
        _state.AddNotAsked();
        _state.AddNotAsked();

        Assert.That(_state.NotAsked, Is.EqualTo(3));
    }

    // ================= Finished =================

    [Test]
    public void Finished_WhenDayReaches100()
    {
        for (int i = 0; i < SemesterState.TotalDays; i++)
            _state.NextDay();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.Finished, Is.True);
            Assert.That(_state.Expelled, Is.False);
        }

    }

    [Test]
    public void Finished_NotBefore100Days()
    {
        for (int i = 0; i < SemesterState.TotalDays - 1; i++)
            _state.NextDay();

        Assert.That(_state.Finished, Is.False);
    }

    // ================= AveragePerDay =================

    [Test]
    public void AveragePerDay_ZeroOnDayZero()
    {
        Assert.That(_state.AveragePerDay, Is.EqualTo(0));
    }

    [Test]
    public void AveragePerDay_DividesByDay()
    {
        _state.NextDay();
        _state.NextDay();
        _state.AddPie();
        _state.AddPie();
        _state.AddPie();
        _state.AddPie();

        Assert.That(_state.AveragePerDay, Is.EqualTo(2.0));
    }

    // ================= Restore =================

    [Test]
    public void Restore_CopiesAllFields()
    {
        var id = Guid.NewGuid();
        var dto = new SessionDto(
            Id: id,
            StudentId: "student-1",
            SessionSeed: 42,
            CurrentDay: 7,
            Utility: 15,
            Skips: 3,
            Expelled: false,
            Finished: false);

        _state.Restore(dto);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.SessionId, Is.EqualTo(id));
            Assert.That(_state.Day, Is.EqualTo(7));
            Assert.That(_state.Utility, Is.EqualTo(15));
            Assert.That(_state.Skips, Is.EqualTo(3));
            Assert.That(_state.Expelled, Is.False);
        }
    }

    [Test]
    public void Restore_Expelled_ResetsNothing()
    {
        var dto = new SessionDto(
            Guid.NewGuid(), "s", 1, 5, 0, 2, Expelled: true, Finished: false);

        _state.Restore(dto);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_state.Expelled, Is.True);
            Assert.That(_state.Utility, Is.EqualTo(0));
        }

    }

    // ================= ToReq =================

    [Test]
    public void ToReq_ReturnsCurrentValues()
    {
        _state.Restore(new SessionDto(Guid.NewGuid(), "s", 1, 5, 20, 4, false, false));

        var req = _state.ToReq();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(req.CurrentDay, Is.EqualTo(5));
            Assert.That(req.Utility, Is.EqualTo(20.0));
            Assert.That(req.Skips, Is.EqualTo(4));
            Assert.That(req.Expelled, Is.False);
            Assert.That(req.Finished, Is.False);
        }
    }

    [Test]
    public void ToReq_FinishedTrue_WhenExpelled()
    {
        _state.Expel();
        var req = _state.ToReq();
        Assert.That(req.Finished, Is.True);
    }
}
