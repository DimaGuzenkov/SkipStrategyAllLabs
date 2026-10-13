using Contracts;
using Polly;
using StudentService;
using StudentService.Clients;
using StudentService.Core;
using StudentService.Infrastructure;
using StudentService.Student;

var builder = Host.CreateApplicationBuilder(args);

var resume = AskUserWantsToResume();

builder.Services.AddSingleton(new StartupOptions { ResumeIfPossible = resume });

builder.Services
    .AddOptions<EndpointsOptions>()
    .Bind(builder.Configuration.GetSection(EndpointsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddStudentClients().AddStudentCore();

var host = builder.Build();
await host.RunAsync();

static bool AskUserWantsToResume()
{
    while (true)
    {
        Console.WriteLine("(N) Новый семестр или (R) Продолжить?");
        var line = Console.ReadLine()?.Trim().ToUpperInvariant();
        if (line == "N") return false;
        if (line == "R") return true;
    }
}

static string AskStudentId()
{
    while (true)
    {
        Console.Write("Введите ID студента: ");
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(input)) return input;
    }
}
