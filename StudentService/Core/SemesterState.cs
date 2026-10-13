using Contracts;
namespace StudentService.Core;

public sealed class SemesterState
{
    public const int TotalDays = 100;
    public const int SubjectCount = 6;

    public Guid SessionId { get; private set; }
    public int Day { get; private set; }
    public int Utility { get; private set; }
    public int Skips { get; private set; }
    public bool Expelled { get; private set; }
    public bool Finished => Expelled || Day == TotalDays;
    public int NotAsked { get; private set; }

    public void AddPie() => Utility++;
    public void AddSkip(int count = 1) { Utility+=count; Skips+=count; }
    public void Expel() { Expelled = true; Utility = 0; }
    public void NextDay() => Day++;
    public void AddNotAsked() => NotAsked++;

    public void Reset()
    {
        Day = 0;
        Utility = 0;
        Skips = 0;
        Expelled = false;
        NotAsked = 0;
    }

    public void Restore(SessionDto dto)
    {
        SessionId = dto.Id;
        Day = dto.CurrentDay;
        Utility = dto.Utility;
        Skips = dto.Skips;
        Expelled = dto.Expelled;
    }

    public UpdateSessionRequest ToReq() => new(Day, Utility, Skips, Expelled, Finished);
    public double AveragePerDay => Day > 0 ? (double)Utility / Day : 0;
}
