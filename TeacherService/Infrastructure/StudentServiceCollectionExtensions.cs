using Contracts;
using MassTransit;
using Microsoft.Extensions.Options;
using Polly;
using TeacherService.api;
using TeacherService.Core.VisitCheckers;
using TeacherService.Core;
using TeacherService.Persistence;
using Microsoft.EntityFrameworkCore;
namespace TeacherService.Infrastructure;

public static class TeacherServiceCollectionExtensions
{
    public static IServiceCollection AddTeacherSubject(this IServiceCollection services)
    {
        var name = Environment.GetEnvironmentVariable(EnvKeys.Subject)
            ?? throw new InvalidOperationException(
                $"Переменная окружения {EnvKeys.Subject} обязательна");

        if (!Enum.TryParse<Subject>(name, ignoreCase: true, out var subject))
        {
            throw new InvalidOperationException(
                $"Неизвестное значение {EnvKeys.Subject}='{name}'. " +
                $"Допустимые: {string.Join(", ", Enum.GetNames<Subject>())}");
        }


        return services.AddSingleton(new SubjectContext(subject));
    }

    public static IServiceCollection AddTeacherPersistence(
        this IServiceCollection services)
    {
        services.AddDbContext<TeacherDbContext>((sp, o) => {
            var opts = sp.GetRequiredService<IOptions<DbOptions>>().Value;
            o.UseSqlite(opts.TeacherDb);
        });
        return services;
    }

    public static IServiceCollection AddTeacherMessaging(this IServiceCollection services)
    {
        services.AddMassTransit(x => x.UsingRabbitMq((sp, cfg) =>
        {
            var opts = sp.GetRequiredService<IOptions<RabbitOptions>>().Value;
            cfg.Host(opts.Host, "/", h =>
            {
                h.Username(opts.User);
                h.Password(opts.Pass);
            });

            cfg.UseMessageRetry(r => r.Exponential(
                retryLimit: 5,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(1)));
        }));
        return services;
    }

    public static IServiceCollection AddTeacherClients(
        this IServiceCollection services)
    {
        services
            .AddHttpClient("history", (sp, c) =>
            {
                var opts = sp.GetRequiredService<IOptions<EndpointsOptions>>().Value;
                c.BaseAddress = new Uri(opts.HistoryUrl);
                c.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddDefaultResilience(clientName: "history")
            .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        services.AddScoped<IHistory>(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("history");
            var publish = sp.GetRequiredService<IPublishEndpoint>();
            return new HistoryClient(http, publish);
        });

        return services;
    }

    public static IServiceCollection AddTeacherDomain(this IServiceCollection services)
    {
        services.AddScoped<ITeacherRuleRepository, TeacherRuleRepository>();
        services.AddScoped<CheckerCreator>();
        services.AddScoped<ITeacher, Teacher>();
        services.AddSingleton<ICounterRandom, CounterRandom>();
        return services;
    }
}
