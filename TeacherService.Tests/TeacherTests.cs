using TeacherService.Core;
using TeacherService.Core.VisitCheckers;
using TeacherService.Persistence;
namespace TeacherService.Tests;

using Contracts;
using NUnit.Framework;
using TeacherService.Tests.Fakes;


[TestFixture]
public class TeacherTests
{
    private static readonly Guid Session = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private FakeHistory _history = null!;
    private FakeTeacherRuleRepository _repo = null!;
    private Teacher _teacher = null!;
    private Subject _subject;

    [SetUp]
    public void Setup()
    {
        _subject = Subject.Calculus;
        _history = new FakeHistory();
        _repo = new FakeTeacherRuleRepository();

        // Предзаполняем config: AChecker на subject 2 (DiscreteMath)
        // Значит, WillAsk возвращает yesterday[DiscreteMath].
        _repo.Seed(Session, new TeacherRuleEntity
        {
            SessionId = Session,
            RuleKind = 1,
            A = (int)Subject.DiscreteMath
        });

        var creator = new CheckerCreator(new CounterRandom(42));
        var subjectContext = new SubjectContext(_subject);

        _teacher = new Teacher(subjectContext, _history, _repo, creator);
    }

    [Test]
    public async Task WasAskedAsync_ReadsYesterdayFromHistory()
    {
        _history.GetDayResponse = new DaySnapshot(0b111111);

        await _teacher.WasAskedAsync(Session, day: 5, CancellationToken.None);

        Assert.That(_history.GetDayCalls, Has.Count.EqualTo(1));
        Assert.That(_history.GetDayCalls[0], Is.EqualTo((Session, 4)),
            "Учитель должен читать day - 1");
    }

    [Test]
    public async Task WasAskedAsync_Day1_ReadsDay0()
    {
        await _teacher.WasAskedAsync(Session, day: 1, CancellationToken.None);

        Assert.That(_history.GetDayCalls[0].Day, Is.EqualTo(0));
    }

    [Test]
    public async Task WasAskedAsync_ReturnsCheckerResult()
    {
        // AChecker(A = DiscreteMath). yesterday[DiscreteMath] = 1 → ask = true
        _history.GetDayResponse = new DaySnapshot().With(Subject.DiscreteMath, true);

        var ask = await _teacher.WasAskedAsync(Session, day: 5, CancellationToken.None);

        Assert.That(ask, Is.True);
    }

    [Test]
    public async Task WasAskedAsync_ReturnsFalse_WhenBitNotSet()
    {
        _history.GetDayResponse = new DaySnapshot(0);

        var ask = await _teacher.WasAskedAsync(Session, day: 5, CancellationToken.None);

        Assert.That(ask, Is.False);
    }

    [Test]
    public async Task WasAskedAsync_WritesToHistory_WithCorrectSubject()
    {
        _history.GetDayResponse = new DaySnapshot(0);

        await _teacher.WasAskedAsync(Session, day: 5, CancellationToken.None);

        Assert.That(_history.RecordCalls, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_history.RecordCalls[0].Subject, Is.EqualTo(_subject));
            Assert.That(_history.RecordCalls[0].Day, Is.EqualTo(5));
            Assert.That(_history.RecordCalls[0].SessionId, Is.EqualTo(Session));
        }

    }

    [Test]
    public async Task WasAskedAsync_WritesAskedValue()
    {
        _history.GetDayResponse = new DaySnapshot().With(Subject.DiscreteMath, true);

        await _teacher.WasAskedAsync(Session, day: 5, CancellationToken.None);

        Assert.That(_history.RecordCalls[0].Asked, Is.True);
    }

    [Test]
    public async Task NotAsked_IncrementsOnFalse()
    {
        _history.GetDayResponse = new DaySnapshot(0);

        await _teacher.WasAskedAsync(Session, 1, CancellationToken.None);
        await _teacher.WasAskedAsync(Session, 2, CancellationToken.None);

        Assert.That(_teacher.NotAsked, Is.EqualTo(2));
    }

    [Test]
    public async Task NotAsked_DoesNotIncrementOnTrue()
    {
        _history.GetDayResponse = new DaySnapshot().With(Subject.DiscreteMath, true);

        await _teacher.WasAskedAsync(Session, 1, CancellationToken.None);
        await _teacher.WasAskedAsync(Session, 2, CancellationToken.None);

        Assert.That(_teacher.NotAsked, Is.EqualTo(0));
    }

    [Test]
    public async Task WasAskedAsync_ConfigExists_DoesNotCreateNew()
    {
        _history.GetDayResponse = new DaySnapshot(0);

        await _teacher.WasAskedAsync(Session, 1, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_repo.LoadCalls, Has.Count.EqualTo(1));
            Assert.That(_repo.SaveCalls, Is.Empty, "Config уже есть — сохранять не нужно");
        }

    }

    [Test]
    public async Task WasAskedAsync_ConfigMissing_CreatesAndSaves()
    {
        var newSession = Guid.NewGuid();
        _history.GetDayResponse = new DaySnapshot(0);

        await _teacher.WasAskedAsync(newSession, 1, CancellationToken.None);

        Assert.That(_repo.SaveCalls, Has.Count.EqualTo(1));
        Assert.That(_repo.SaveCalls[0].SessionId, Is.EqualTo(newSession));
    }

    [Test]
    public async Task WasAskedAsync_ConfigMissing_NextCallUsesSameConfig()
    {
        var newSession = Guid.NewGuid();
        _history.GetDayResponse = new DaySnapshot(0);

        await _teacher.WasAskedAsync(newSession, 1, CancellationToken.None);
        var savedConfig = _repo.SaveCalls[0];

        await _teacher.WasAskedAsync(newSession, 2, CancellationToken.None);

        // Второй вызов не должен создавать новый config
        Assert.That(_repo.SaveCalls, Has.Count.EqualTo(1));
    }
}
