namespace TeacherService.Persistence;

public sealed class TeacherRuleEntity
{
    public Guid SessionId { get; set; }
    public int RuleKind { get; set; }
    public int A { get; set; } = -1;
    public int B { get; set; } = -1;
    public ulong Seed { get; set; }
}
