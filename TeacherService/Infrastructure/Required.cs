using Contracts;
namespace TeacherService.Infrastructure;

public sealed record EndpointsOptions
{
    public const string SectionName = "Endpoints";

    public required string HistoryUrl { get; init; }
}

public sealed record RabbitOptions
{
    public const string SectionName = "RabbitMq";

    public required string Host { get; init; } = "rabbitmq";
    public required string User { get; init; } = "dev";
    public required string Pass { get; init; } = "dev";
}

public sealed record DbOptions
{
    public const string SectionName = "ConnectionStrings";

    public required string TeacherDb { get; init; }
}

public static class EnvKeys
{
        public const string Subject = "SUBJECT";
}
