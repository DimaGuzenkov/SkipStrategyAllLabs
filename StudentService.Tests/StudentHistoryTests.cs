namespace StudentService.Tests;

using Contracts;
using NUnit.Framework;
using StudentService.Core;

[TestFixture]
public class StudentHistoryTests
{
    private StudentHistory _history = null!;

    [SetUp]
    public void Setup() => _history = new StudentHistory();

    // ================= Начальное состояние =================

    [Test]
    public void Empty_AllAttendedFalse()
    {
        using (Assert.EnterMultipleScope())
        {
            for (int day = 1; day <= 5; day++)
            {
                foreach (Subject s in Enum.GetValues<Subject>())
                {
                    Assert.That(_history.Attended(day, s), Is.False);
                }
            }
        }
    }

    [Test]
    public void Empty_CurrentDayZero()
    {
        Assert.That(_history.CurrentDay, Is.EqualTo(0));
    }

    // ================= RecordAttended =================

    [Test]
    public void RecordAttended_SetsBit()
    {
        _history.RecordAttended(1, Subject.Physics, true);

        Assert.That(_history.Attended(1, Subject.Physics), Is.True);
    }

    [Test]
    public void RecordAttended_IgnoresOtherSubjects()
    {
        _history.RecordAttended(1, Subject.Physics, true);

        Assert.That(_history.Attended(1, Subject.Calculus), Is.False);
    }

    [Test]
    public void RecordAttended_MultipleSubjects_Accumulates()
    {
        _history.RecordAttended(1, Subject.Calculus, true);
        _history.RecordAttended(1, Subject.Physics, true);
        _history.RecordAttended(1, Subject.English, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_history.Attended(1, Subject.Calculus), Is.True);
            Assert.That(_history.Attended(1, Subject.Physics), Is.True);
            Assert.That(_history.Attended(1, Subject.English), Is.True);
        }

    }

    [Test]
    public void RecordAttended_UpdatesCurrentDay()
    {
        _history.RecordAttended(5, Subject.Calculus, true);
        Assert.That(_history.CurrentDay, Is.EqualTo(5));
    }

    [Test]
    public void RecordAttended_DoesNotDecreaseCurrentDay()
    {
        _history.RecordAttended(5, Subject.Calculus, true);
        _history.RecordAttended(2, Subject.Calculus, true);

        Assert.That(_history.CurrentDay, Is.EqualTo(5));
    }

    // ================= RecordAsked =================

    [Test]
    public void RecordAsked_SetsBit()
    {
        _history.RecordAsked(1, Subject.Physics, true);

        Assert.That(_history.WasAsked(1, Subject.Physics), Is.True);
    }

    [Test]
    public void WasAsked_NeverNull()
    {
        // WasAsked возвращает bool? но всегда имеет конкретное значение
        Assert.That(_history.WasAsked(1, Subject.Calculus), Is.False);
        Assert.That(_history.WasAsked(1, Subject.Calculus), Is.Not.Null);
    }

    // ================= Record(day, subject, attended, asked) =================

    [Test]
    public void Record_SetsBothMasks()
    {
        _history.Record(1, Subject.Physics, attended: true, asked: false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_history.Attended(1, Subject.Physics), Is.True);
            Assert.That(_history.WasAsked(1, Subject.Physics), Is.False);
        }

    }

    [Test]
    public void Record_UpdatesCurrentDay()
    {
        _history.Record(3, Subject.Physics, true, true);
        Assert.That(_history.CurrentDay, Is.EqualTo(3));
    }

    // ================= Record(List<DaySnapshot>) — восстановление =================

    [Test]
    public void RecordDays_FillsAsked()
    {
        var days = new List<DaySnapshot>
        {
            new(0b000001),   // день 1
            new(0b000010),   // день 2
            new(0b000100),   // день 3
        };

        _history.Record(days);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_history.WasAsked(1, Subject.Calculus), Is.True);
            Assert.That(_history.WasAsked(2, Subject.LinearAlgebra), Is.True);
            Assert.That(_history.WasAsked(3, Subject.DiscreteMath), Is.True);
        }

    }

    [Test]
    public void RecordDays_SetsCurrentDay()
    {
        var days = new List<DaySnapshot>
        {
            new(0b000001),
            new(0b000010),
            new(0b000100),
        };

        _history.Record(days);

        Assert.That(_history.CurrentDay, Is.EqualTo(3));
    }

    [Test]
    public void RecordDays_DoesNotRestoreAttended()
    {
        // attended не хранится в истории, только asked.
        // После Record(days) attended должен быть пустым.
        _history.Record([new(0b111111)]);

        Assert.That(_history.Attended(1, Subject.Calculus), Is.False);
    }

    // ================= IsExpelled =================

    [Test]
    public void IsExpelled_NoAttendedNoAsked_False()
    {
        Assert.That(_history.IsExpelled(1), Is.False);
    }

    [Test]
    public void IsExpelled_AllAttendedNoneAsked_False()
    {
        _history.RecordAttended(1, new DaySnapshot(0b111111));
        _history.RecordAsked(1, new DaySnapshot(0b000000));

        Assert.That(_history.IsExpelled(1), Is.False);
    }

    [Test]
    public void IsExpelled_AllAttendedAllAsked_False()
    {
        _history.RecordAttended(1, new DaySnapshot(0b111111));
        _history.RecordAsked(1, new DaySnapshot(0b111111));

        Assert.That(_history.IsExpelled(1), Is.False);
    }

    [Test]
    public void IsExpelled_SkippedNotAsked_False()
    {
        // attended: только предмет 0 (Calculus)
        // asked:    только предмет 0
        // пропустили остальных, но их не спрашивали — не вылет
        _history.RecordAttended(1, new DaySnapshot(0b000001));
        _history.RecordAsked(1, new DaySnapshot(0b000001));

        Assert.That(_history.IsExpelled(1), Is.False);
    }

    [Test]
    public void IsExpelled_SkippedAndAsked_True()
    {
        // attended: ни на кого не пошли
        // asked: спросили Calculus
        // → вылет
        _history.RecordAttended(1, new DaySnapshot(0b000000));
        _history.RecordAsked(1, new DaySnapshot(0b000001));

        Assert.That(_history.IsExpelled(1), Is.True);
    }

    [Test]
    public void IsExpelled_AttendedSubject3_SkippedAskedSubject5_True()
    {
        // attended: предмет 3 (Programming)
        // asked: предмет 3 и предмет 5 (English)
        // → вылет из-за пропущенного предмета 5
        _history.RecordAttended(1, new DaySnapshot(0b001000));
        _history.RecordAsked(1, new DaySnapshot(0b100000 | 0b001000));

        Assert.That(_history.IsExpelled(1), Is.True);
    }

    [Test]
    public void IsExpelled_AttendedAskedPlusExtra_False()
    {
        // attended: предметы 0, 2
        // asked: только предмет 0
        // пропустили предмет 2, но там не спросили — не вылет
        _history.RecordAttended(1, new DaySnapshot(0b000101));
        _history.RecordAsked(1, new DaySnapshot(0b000001));

        Assert.That(_history.IsExpelled(1), Is.False);
    }
}
