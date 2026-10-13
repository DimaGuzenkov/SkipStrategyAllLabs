
using Contracts;
using TeacherService.Persistence;
using TeacherService.Core;
using TeacherService.Core.VisitCheckers;

namespace TeacherService.Core;

public class Teacher(
    SubjectContext subject,
    IHistory history,
    ITeacherRuleRepository repo,
    CheckerCreator creator
) : ITeacher
{
    private readonly Subject subject = subject.Subject;

    private readonly IHistory history = history;
    private readonly ITeacherRuleRepository _repo = repo;
    private readonly CheckerCreator _creator = creator;
    public int NotAsked { get; private set; } = 0;

    public async Task<bool> WasAskedAsync(Guid sessionId, int day, CancellationToken ct)
    {
        var yesterday = await history.GetDayAsync(sessionId, day - 1, ct);
        var checker = await GetOrCreateCheckerAsync(sessionId, ct);
        var ask = checker.IsChecking(day, yesterday);
        if (!ask) NotAsked++;
        await history.RecordAsync(sessionId, day, subject, ask, ct);
        return ask;
    }

    private async Task<IVisitChecker> GetOrCreateCheckerAsync(Guid sessionId, CancellationToken ct)
    {
        var config = await _repo.LoadAsync(sessionId, ct);
        if (config is null)
        {
            config = _creator.CreateCheckerConfig(sessionId);
            await _repo.SaveAsync(sessionId, config, ct);
        }
        return CheckerCreator.CreateByConfig(config);
    }
}
