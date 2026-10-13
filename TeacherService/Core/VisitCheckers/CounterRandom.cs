namespace TeacherService.Core.VisitCheckers;

public interface ICounterRandom
{
    ulong Seed { get; }
    int NextInt(ulong index);
    int NextInt(ulong index, int max);
}

public class CounterRandom : ICounterRandom
{
    public ulong Seed { get; init; } = 0;

    public CounterRandom(ulong seed) => Seed = seed;

    public CounterRandom(SubjectContext subject)
    {
        Seed = NextUInt64((ulong)subject.Subject);
    }

    public ulong NextUInt64(ulong index)
    {
        ulong z = Seed + (index * 0x9E3779B97F4A7C15UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    public int NextInt(ulong index) => (int)NextUInt64(index);

    public int NextInt(ulong index, int max)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max);
        return (int)(NextUInt64(index) % (ulong)max);
    }

}
