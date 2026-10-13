using System.Numerics;
namespace Contracts;

public readonly struct DaySnapshot(byte mask) : IEquatable<DaySnapshot>
{
    public byte Mask { get; } = mask;
    public bool this[Subject s] => (Mask & (1 << (int)s)) != 0;

    public DaySnapshot With(Subject s, bool value)
    {
        int bit = 1 << (int)s;
        return new DaySnapshot(value
            ? (byte)(Mask | bit)
            : (byte)(Mask & ~bit));
    }

    public DaySnapshot(bool[] values) : this(Pack(values)) { }

    private static byte Pack(bool[] values)
    {
        byte m = 0;
        for (int i = 0; i < values.Length; i++)
            if (values[i]) m |= (byte)(1 << i);
        return m;
    }

    public int Count => BitOperations.PopCount(Mask);

    public bool Equals(DaySnapshot other) => Mask == other.Mask;
    public override bool Equals(object? o) => o is DaySnapshot d && Equals(d);
    public override int GetHashCode() => Mask;
    public override string ToString() => Convert.ToString(Mask, 2).PadLeft(6, '0');

    public static bool operator ==(DaySnapshot left, DaySnapshot right) => left.Equals(right);
    public static bool operator !=(DaySnapshot left, DaySnapshot right) => !(left == right);

}
