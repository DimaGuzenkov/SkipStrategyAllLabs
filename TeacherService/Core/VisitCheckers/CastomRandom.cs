// namespace Lab4.Teacher.VisitCheckers;

// public class CastomRandom : Random
// {
//     public int Seed { get; init; }
//     public int NextCount { get; private set; }

//     public CastomRandom(int seed, int nextCount = 0) : base(seed)
//     {
//         Seed = seed;
//         NextCount = nextCount;
//         for (int i = 0; i < NextCount; ++i)
//         {
//             _ = Next();
//         }
//     }

//     public override int Next()
//     {
//         NextCount++;
//         return base.Next();
//     }

//     public override int Next(int maxValue)
//     {
//         NextCount++;
//         return base.Next(maxValue);
//     }

//     public override int Next(int minValue, int maxValue)
//     {
//         NextCount++;
//         return base.Next(minValue, maxValue);
//     }

// }
