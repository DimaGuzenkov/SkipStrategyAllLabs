namespace Contracts;

public enum Subject { Calculus, LinearAlgebra, DiscreteMath, Programming, Physics, English }

public record AskRequest(Guid SessionId, int Day);
public record AskResponse(bool Asked);

public record DayRecord(int Day, byte Mask);
public record SessionDaysResponse(IReadOnlyList<DayRecord> Days);

public record RecordAskedRequest(Guid SessionId, int Day, int Subject, bool Asked);

public record DaySnapshotResponse(byte Mask);

public record SessionDto(
    Guid Id,
    string StudentId,
    long SessionSeed,
    int CurrentDay,
    int Utility,
    int Skips,
    bool Expelled,
    bool Finished);

public record CreateSessionRequest(string StudentId);

public record UpdateSessionRequest(
    int CurrentDay,
    int Utility,
    int Skips,
    bool Expelled,
    bool Finished);
