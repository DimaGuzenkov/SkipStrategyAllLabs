using Contracts;
using TeacherService.Persistence;
using TeacherService.Core.VisitCheckers;
namespace TeacherService.Core.VisitCheckers;

public class CheckerCreator(ICounterRandom rnd)
{
    private const int SubjectCount = 6;
    private readonly ICounterRandom _rnd = rnd;
    private static readonly Subject[] subjects = Enum.GetValues<Subject>();

    public TeacherRuleEntity CreateCheckerConfig(Guid id)
    {
        byte[] bytes = id.ToByteArray();

        ulong part1 = BitConverter.ToUInt64(bytes, 0);
        ulong part2 = BitConverter.ToUInt64(bytes, 8);
        var config = new TeacherRuleEntity
        {
            SessionId = id,
            RuleKind = _rnd.NextInt(part1, 3)
        };

        switch (config.RuleKind)
        {
            case 0: config.Seed = (ulong)_rnd.NextInt(part2); break;
            case 1: config.A = _rnd.NextInt(part2, SubjectCount); break;
            default:
                config.A = _rnd.NextInt(part2, SubjectCount);
                do { config.B = _rnd.NextInt(part2 *= 2, SubjectCount); } while (config.B == config.A);
                break;
        }

        return config;
    }

    public static IVisitChecker CreateByConfig(TeacherRuleEntity conf) => conf.RuleKind switch
    {
        0 => new RandomChecker(new CounterRandom(conf.Seed)),
        1 => new AChecker(subjects[conf.A]),
        _ => new ABChecker(subjects[conf.A], subjects[conf.B])
    };
}
