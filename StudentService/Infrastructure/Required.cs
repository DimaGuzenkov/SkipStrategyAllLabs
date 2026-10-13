using Contracts;
namespace StudentService.Infrastructure;

public sealed record EndpointsOptions
{
    public const string SectionName = "Endpoints";

    public required string SessionUrl { get; init; }
    public required string HistoryUrl { get; init; }
    public required Dictionary<Subject, string> Teachers { get; init; }
}
