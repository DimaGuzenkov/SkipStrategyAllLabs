namespace StudentService.Tests;

using Contracts;
using NUnit.Framework;
using StudentService.Core;
using StudentService.Student;
using StudentService.Tests.Fakes;

[TestFixture]
public class ResumeTests
{
    private static readonly Guid Session =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static bool DeterministicAsked(Guid sessionId, int day, Subject subject)
    {
        unchecked
        {
            ulong h = 17;
            foreach (var b in sessionId.ToByteArray()) h = (h * 31) + b;
            h = (h * 31) + (ulong)day;
            h = (h * 31) + (ulong)(int)subject;
            return (h % 5) == 0;
        }
    }

    private static List<DaySnapshot> SnapshotHistory(IReadOnlyStudentHistory h, int upToDay)
    {
        var result = new List<DaySnapshot>();
        for (int d = 1; d <= upToDay; d++)
        {
            byte mask = 0;
            foreach (var s in Enum.GetValues<Subject>())
            {

                if (h.WasAsked(d, s) is true)
                {
                    mask |= (byte)(1 << (int)s);
                }
            }

            result.Add(new DaySnapshot(mask));
        }
        return result;
    }

    private static SemesterOrchestrator BuildOrchestrator(
        Func<Guid, int, Subject, bool> teacherFunc,
        out SemesterState state,
        out StudentHistory history)
    {
        state = new SemesterState();
        history = new StudentHistory();
        var orch = new SemesterOrchestrator(
            new FakeTeacher(teacherFunc),
            new Strategy(),
            history);
        return orch;
    }

    // =================================================================
    // 1. Непрерывный прогон vs Прогон с прерыванием
    // =================================================================

    [Test]
    public async Task InterruptedAndResumed_MatchesContinuous()
    {
        // --- A. Непрерывный прогон 100 дней ---
        var orchA = BuildOrchestrator(DeterministicAsked, out var stateA, out _);
        stateA.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orchA.SetHistory(stateA, []);

        for (int i = 0; i < 100 && !stateA.Finished; i++)
            await orchA.RunDayAsync(CancellationToken.None);

        // --- B.1. Прогон 50 дней ---
        var orchB1 = BuildOrchestrator(DeterministicAsked, out var stateB1, out var historyB1);
        stateB1.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orchB1.SetHistory(stateB1, []);

        for (int i = 0; i < 50 && !stateB1.Finished; i++)
            await orchB1.RunDayAsync(CancellationToken.None);

        // --- B.2. Снимок состояния ---
        var savedDto = new SessionDto(
            Session, "s", 1,
            stateB1.Day, stateB1.Utility, stateB1.Skips,
            stateB1.Expelled, stateB1.Finished);
        var savedHistory = SnapshotHistory(historyB1, stateB1.Day);

        // --- B.3. "Рестарт": новые экземпляры, восстановление из снимка ---
        var orchB2 = BuildOrchestrator(DeterministicAsked, out var stateB2, out _);
        stateB2.Restore(savedDto);
        orchB2.SetHistory(stateB2, savedHistory);

        // Продолжаем до конца
        for (int i = stateB2.Day; i < 100 && !stateB2.Finished; i++)
            await orchB2.RunDayAsync(CancellationToken.None);

        // --- Сравнение ---
        using (Assert.EnterMultipleScope())
        {
            Assert.That(stateB2.Day, Is.EqualTo(stateA.Day),
                "Конечный день должен совпасть");
            Assert.That(stateB2.Utility, Is.EqualTo(stateA.Utility),
                "Utility должна совпасть");
            Assert.That(stateB2.Skips, Is.EqualTo(stateA.Skips),
                "Skips должны совпасть");
            Assert.That(stateB2.Expelled, Is.EqualTo(stateA.Expelled),
                "Флаг вылета должен совпасть");
        }
    }

    // =================================================================
    // 2. Стратегия: восстановление из истории = непрерывная работа
    // =================================================================

    [Test]
    public void Strategy_ResumeFromHistory_MatchesContinuous()
    {
        const int interruptedAt = 50;

        // Синтетическая история: случайные asked за 50 дней
        var rng = new Random(42);
        var history = new FakeStudentHistory();
        for (int d = 1; d <= interruptedAt; d++)
        {
            foreach (var s in Enum.GetValues<Subject>())
            {
                history.Set(d, s, rng.Next(2) == 1);
            }
        }

        // --- A. Непрерывно: один экземпляр, прогон всех дней ---


        var stratA = new Strategy();
        bool[] decisionsA = null!;
        for (int d = 1; d <= interruptedAt + 1; d++)
            decisionsA = stratA.DecideDay(d, history);

        // --- B. "Рестарт": свежий экземпляр, первый вызов на день interruptedAt + 1 ---
        var stratB = new Strategy();
        var decisionsB = stratB.DecideDay(interruptedAt + 1, history);

        // --- Сравнение ---
        Assert.That(decisionsB, Is.EqualTo(decisionsA),
            "Свежая стратегия должна дать то же решение, что и накопленная");
    }

    [Test]
    public void Strategy_ResumeAtDifferentPoints_ConsistentWithContinuous()
    {
        // Проверяем, что стратегия даёт одинаковый результат независимо от того,
        // на каком дне её "перезапустили" — если история одна и та же.

        var rng = new Random(123);
        var history = new FakeStudentHistory();
        for (int d = 1; d <= 30; d++)
        {
            foreach (var s in Enum.GetValues<Subject>())
            {
                history.Set(d, s, rng.Next(2) == 1);
            }
        }

        // Непрерывный прогон до дня 31


        var continuous = new Strategy();
        bool[] expected = null!;
        for (int d = 1; d <= 31; d++)
            expected = continuous.DecideDay(d, history);

        // Перезапуск в разных точках (все — свежие экземпляры, первый вызов на 31)
        for (int trial = 0; trial < 5; trial++)
        {
            var fresh = new Strategy();
            var actual = fresh.DecideDay(31, history);

            Assert.That(actual, Is.EqualTo(expected),
                $"Прогон #{trial}: свежий экземпляр дал другое решение");
        }
    }

    // =================================================================
    // 3. Сохранение / восстановление SessionDto — идентичность
    // =================================================================

    [Test]
    public void SessionState_SaveAndRestore_PreservesFields()
    {
        var original = new SemesterState();
        original.Restore(new SessionDto(Session, "student-1", 42, 10, 55, 7, false, false));

        // "Сохраняем" в DTO
        var saved = new SessionDto(
            original.SessionId,
            "student-1",
            42,
            original.Day,
            original.Utility,
            original.Skips,
            original.Expelled,
            original.Finished);

        // "Восстанавливаем"
        var restored = new SemesterState();
        restored.Restore(saved);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(restored.SessionId, Is.EqualTo(original.SessionId));
            Assert.That(restored.Day, Is.EqualTo(original.Day));
            Assert.That(restored.Utility, Is.EqualTo(original.Utility));
            Assert.That(restored.Skips, Is.EqualTo(original.Skips));
            Assert.That(restored.Expelled, Is.EqualTo(original.Expelled));
        }
    }

    [Test]
    public void StudentHistory_SaveAndRestore_PreservesAskedMasks()
    {
        // Заполняем оригинал
        var original = new StudentHistory();
        for (int d = 1; d <= 10; d++)
            original.RecordAsked(d, new DaySnapshot((byte)(d * 3)));

        // "Сохраняем": забираем маски
        var saved = SnapshotHistory(original, 10);

        // "Восстанавливаем": новый экземпляр + Record(List<DaySnapshot>)
        var restored = new StudentHistory();
        restored.Record(saved);

        // Сравниваем
        for (int d = 1; d <= 10; d++)
        {
            for (int s = 0; s < 6; s++)
            {
                var expected = original.WasAsked(d, (Subject)s);
                var actual = restored.WasAsked(d, (Subject)s);
                Assert.That(actual, Is.EqualTo(expected),
                    $"День {d}, предмет {(Subject)s}: ожидалось {expected}, получено {actual}");
            }
        }
    }

    // =================================================================
    // 4. Двойное прерывание
    // =================================================================

    [Test]
    public async Task TwoInterruptions_MatchesContinuous()
    {
        // A. Непрерывный
        var orchA = BuildOrchestrator(DeterministicAsked, out var stateA, out _);
        stateA.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orchA.SetHistory(stateA, []);
        for (int i = 0; i < 100 && !stateA.Finished; i++)
            await orchA.RunDayAsync(CancellationToken.None);

        // B. Прерывание на 30, потом на 60
        var orchB = BuildOrchestrator(DeterministicAsked, out var stateB, out var historyB);
        stateB.Restore(new SessionDto(Session, "s", 1, 0, 0, 0, false, false));
        orchB.SetHistory(stateB, []);

        // 0..30
        for (int i = 0; i < 30 && !stateB.Finished; i++)
            await orchB.RunDayAsync(CancellationToken.None);

        // Снимок 1, рестарт
        var saved1 = new SessionDto(Session, "s", 1, stateB.Day,
            stateB.Utility, stateB.Skips, stateB.Expelled, stateB.Finished);
        var history1 = SnapshotHistory(historyB, stateB.Day);
        orchB = BuildOrchestrator(DeterministicAsked, out stateB, out historyB);
        stateB.Restore(saved1);
        orchB.SetHistory(stateB, history1);

        // 30..60
        for (int i = stateB.Day; i < 60 && !stateB.Finished; i++)
            await orchB.RunDayAsync(CancellationToken.None);

        // Снимок 2, рестарт
        var saved2 = new SessionDto(Session, "s", 1, stateB.Day,
            stateB.Utility, stateB.Skips, stateB.Expelled, stateB.Finished);
        var history2 = SnapshotHistory(historyB, stateB.Day);
        orchB = BuildOrchestrator(DeterministicAsked, out stateB, out historyB);
        stateB.Restore(saved2);
        orchB.SetHistory(stateB, history2);

        // 60..100
        for (int i = stateB.Day; i < 100 && !stateB.Finished; i++)
            await orchB.RunDayAsync(CancellationToken.None);

        // Сравнение
        using (Assert.EnterMultipleScope())
        {
            Assert.That(stateB.Day, Is.EqualTo(stateA.Day));
            Assert.That(stateB.Utility, Is.EqualTo(stateA.Utility));
            Assert.That(stateB.Skips, Is.EqualTo(stateA.Skips));
            Assert.That(stateB.Expelled, Is.EqualTo(stateA.Expelled));
        }
    }
}
