using HistoryService.Tests.Fakes;

namespace HistoryService.Tests;

[TestFixture]
public class ServiceTests
{
    private static readonly Guid Session = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly TimeSpan ShortTimeout = TimeSpan.FromMilliseconds(200);

    private FakeHistoryRepository _repo = null!;
    private Service _svc = null!;

    [SetUp]
    public void Setup()
    {
        _repo = new FakeHistoryRepository();
        _svc = new Service(_repo, ShortTimeout);
    }

    // ================= GetDayMaskAsync =================

    [Test]
    public async Task GetDayMask_Day0_ReturnsZero_NoRepoCall()
    {
        var mask = await _svc.GetDayMaskAsync(Session, 0, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mask, Is.EqualTo(0));
            Assert.That(_repo.GetDayCalls, Is.Empty, "Для дня 0 в БД ходить не нужно");
        }
    }

    [Test]
    public async Task GetDayMask_ClosedDay_ReturnsMaskImmediately()
    {
        _repo.SetClosedDay(Session, 5, 0b101010);

        var mask = await _svc.GetDayMaskAsync(Session, 5, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mask, Is.EqualTo(0b101010));
            Assert.That(_repo.GetDayCalls, Has.Count.EqualTo(1));
        }

    }

    [Test]
    public async Task GetDayMask_NotClosed_WaitsUntilClosed()
    {
        var getTask = _svc.GetDayMaskAsync(Session, 5, CancellationToken.None);
        await Task.Delay(50);
        Assert.That(getTask.IsCompleted, Is.False, "День не закрыт — ждём");

        for (int s = 0; s < 6; s++)
            await _svc.RecordAsync(Session, 5, s, false, CancellationToken.None);

        var mask = await getTask;
        Assert.That(mask, Is.EqualTo(0));
    }

    [Test]
    public async Task GetDayMask_NotClosed_TimesOut()
    {
        Assert.ThrowsAsync<DayNotClosedException>(async () =>
            await _svc.GetDayMaskAsync(Session, 5, CancellationToken.None));
    }

    [Test]
    public async Task GetDayMask_Cancelled_ThrowsOperationCancelled()
    {
        using var cts = new CancellationTokenSource();
        var getTask = _svc.GetDayMaskAsync(Session, 5, cts.Token);
        await Task.Delay(50);

        cts.Cancel();

        try
        {
            await getTask;
            Assert.Fail("Ожидалось OperationCanceledException");
        }
        catch (OperationCanceledException)
        {}
    }

    // ================= RecordAsync =================

    [Test]
    public async Task RecordAsync_FirstSubject_SetsBit0()
    {
        await _svc.RecordAsync(Session, 5, subject: 0, asked: true, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_repo.GetRecordedMask(Session, 5), Is.EqualTo(1));
            Assert.That(_repo.GetMask(Session, 5), Is.EqualTo(1));
        }

    }

    [Test]
    public async Task RecordAsync_AllSixSubjects_RecordedMaskBecomes63()
    {
        for (int s = 0; s < 6; s++)
            await _svc.RecordAsync(Session, 5, s, true, CancellationToken.None);

        Assert.That(_repo.GetRecordedMask(Session, 5), Is.EqualTo(63));
    }

    [Test]
    public async Task RecordAsync_DuplicateSubject_RecordedMaskUnchanged()
    {
        await _svc.RecordAsync(Session, 5, 0, true, CancellationToken.None);
        var afterFirst = _repo.GetRecordedMask(Session, 5);

        await _svc.RecordAsync(Session, 5, 0, true, CancellationToken.None);
        await _svc.RecordAsync(Session, 5, 0, false, CancellationToken.None);

        Assert.That(_repo.GetRecordedMask(Session, 5), Is.EqualTo(afterFirst),
            "Повторные записи того же subject не меняют recorded_mask");
    }

    [Test]
    public async Task RecordAsync_AskedFalse_ClearsBitInMask()
    {
        await _svc.RecordAsync(Session, 5, 0, asked: true, CancellationToken.None);
        Assert.That(_repo.GetMask(Session, 5), Is.EqualTo(1));

        await _svc.RecordAsync(Session, 5, 0, asked: false, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_repo.GetMask(Session, 5), Is.EqualTo(0));
            Assert.That(_repo.GetRecordedMask(Session, 5), Is.EqualTo(1),
                "recorded_mask остаётся — учитель уже отписался");
        }

    }

    // ================= Интеграция =================

    [Test]
    public async Task RecordAsync_WhenDayCloses_WakesWaiter()
    {
        var getTask = _svc.GetDayMaskAsync(Session, 5, CancellationToken.None);
        await Task.Delay(50);
        Assert.That(getTask.IsCompleted, Is.False);

        for (int s = 0; s < 6; s++)
            await _svc.RecordAsync(Session, 5, s, asked: s % 2 == 0, CancellationToken.None);

        var mask = await getTask;
        Assert.That(mask, Is.EqualTo(0b010101), "Биты 0, 2, 4 = 1+4+16 = 21 = 0b010101");
    }

    [Test]
    public async Task RecordAsync_NoWaiter_DoesNotThrow()
    {
        for (int s = 0; s < 6; s++)
        {
            var s2 = s;
            Assert.DoesNotThrowAsync(async () =>
                await _svc.RecordAsync(Session, 5, s2, true, CancellationToken.None));
        }
    }

    [Test]
    public async Task RecordAsync_TwoSessions_Independent()
    {
        var session2 = Guid.NewGuid();

        for (int s = 0; s < 6; s++)
        {
            await _svc.RecordAsync(Session, 1, s, true, CancellationToken.None);
            await _svc.RecordAsync(session2, 1, s, false, CancellationToken.None);
        }

        var mask1 = await _svc.GetDayMaskAsync(Session, 1, CancellationToken.None);
        var mask2 = await _svc.GetDayMaskAsync(session2, 1, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mask1, Is.EqualTo(63));
            Assert.That(mask2, Is.EqualTo(0));
        }

    }

    [Test]
    public async Task RecordAsync_DayClosesOnce_MultipleRecordCallsAreSafe()
    {
        for (int s = 0; s < 6; s++)
            await _svc.RecordAsync(Session, 5, s, true, CancellationToken.None);

        Assert.DoesNotThrowAsync(async () =>
            await _svc.RecordAsync(Session, 5, 0, true, CancellationToken.None));
    }

    // ================= GetAllClosedDaysAsync =================

    [Test]
    public async Task GetAllClosedDays_ReturnsOnlyClosed_InOrder()
    {
        _repo.SetClosedDay(Session, 1, 0b111111);
        _repo.SetClosedDay(Session, 3, 0b000001);

        await _svc.RecordAsync(Session, 2, 0, true, CancellationToken.None);
        await _svc.RecordAsync(Session, 2, 1, true, CancellationToken.None);

        var days = await _svc.GetAllClosedDaysAsync(Session, CancellationToken.None);

        Assert.That(days, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(days[0].Day, Is.EqualTo(1));
            Assert.That(days[1].Day, Is.EqualTo(3));
        }

    }

    [Test]
    public async Task GetAllClosedDays_EmptySession_ReturnsEmpty()
    {
        var days = await _svc.GetAllClosedDaysAsync(Session, CancellationToken.None);
        Assert.That(days, Is.Empty);
    }
}
