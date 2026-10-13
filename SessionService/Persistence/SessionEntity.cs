namespace SessionService.Persistence;

public sealed class SessionEntity
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = "";
    public long SessionSeed { get; set; }
    public int CurrentDay { get; set; }
    public int Utility { get; set; }
    public int Skips { get; set; }
    public bool Expelled { get; set; }
    public bool Finished { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
