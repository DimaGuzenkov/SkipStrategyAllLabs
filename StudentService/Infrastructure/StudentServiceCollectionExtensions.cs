using Contracts;
using Microsoft.Extensions.Options;
using StudentService.Clients;
using StudentService.Core;
using StudentService.Student;
namespace StudentService.Infrastructure;

public static class StudentServiceCollectionExtensions
{
    public static IServiceCollection AddStudentClients(
        this IServiceCollection services)
    {
        services.AddHttpClient("history", (sp, c) =>
        {
            var opts = sp.GetRequiredService<IOptions<EndpointsOptions>>().Value;
            c.BaseAddress = new Uri(opts.HistoryUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddDefaultResilience(clientName: "history");

        services.AddHttpClient("session", (sp, c) =>
        {
            var opts = sp.GetRequiredService<IOptions<EndpointsOptions>>().Value;
            c.BaseAddress = new Uri(opts.SessionUrl);
        })
        .AddDefaultResilience(clientName: "session");

        foreach (var subject in Enum.GetValues<Subject>())
        {
            services.AddHttpClient($"teacher-{subject}", (sp, c) =>
            {
                var opts = sp.GetRequiredService<IOptions<EndpointsOptions>>().Value;
                c.BaseAddress = new Uri(opts.Teachers[subject]);
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddDefaultResilience(retryCount: 3, clientName: $"teacher-{subject}");
        }

        services.AddSingleton<ISessionService, SessionClient>();
        services.AddSingleton<IHistory, HistoryClient>();
        services.AddSingleton<ITeacher, TeacherClient>();
        return services;
    }

    public static IServiceCollection AddStudentCore(this IServiceCollection services)
    {
        services.AddSingleton<IStudentHistory, StudentHistory>();
        services.AddSingleton<SemesterState>();
        services.AddSingleton<ISkipStrategy, Strategy>();
        services.AddSingleton<SemesterOrchestrator>();
        services.AddHostedService<SemesterLoopService>();
        return services;
    }
}
