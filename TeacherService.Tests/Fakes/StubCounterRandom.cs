namespace TeacherService.Tests.Fakes;

using TeacherService.Core.VisitCheckers;

public sealed class StubCounterRandom(Func<ulong, int, int> impl) : ICounterRandom
{
    private readonly Func<ulong, int, int> _impl = impl;
    public ulong Seed { get; init; }

    public int NextInt(ulong index) => _impl(index, int.MaxValue);
    public int NextInt(ulong index, int max) => _impl(index, max);

    public static StubCounterRandom Fixed(int value) =>
        new((_, max) => value % max);

    public static StubCounterRandom Sequence(params int[] values)
    {
        int i = 0;
        return new((_, max) => values[i++ % values.Length] % max);
    }

    public static (StubCounterRandom Stub, List<ulong> Calls) Tracking(Func<ulong, int, int> impl)
    {
        var calls = new List<ulong>();
        var stub = new StubCounterRandom((idx, max) => { calls.Add(idx); return impl(idx, max); });
        return (stub, calls);
    }
}
