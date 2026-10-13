using TeacherService.Persistence;

namespace TeacherService.Tests.Fakes;

public sealed class FakeTeacherRuleRepository : ITeacherRuleRepository
{
    private readonly Dictionary<Guid, TeacherRuleEntity> _configs = [];

    public List<Guid> LoadCalls { get; } = [];
    public List<TeacherRuleEntity> SaveCalls { get; } = [];

    public Task<TeacherRuleEntity?> LoadAsync(Guid sessionId, CancellationToken ct)
    {
        LoadCalls.Add(sessionId);
        return Task.FromResult(_configs.TryGetValue(sessionId, out var c) ? c : null);
    }

    public Task SaveAsync(Guid sessionId, TeacherRuleEntity config, CancellationToken ct)
    {
        SaveCalls.Add(config);
        _configs[sessionId] = config;
        return Task.CompletedTask;
    }

    /// <summary>Предзаполнить config для сессии.</summary>
    public void Seed(Guid sessionId, TeacherRuleEntity config) => _configs[sessionId] = config;
}
