using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TeacherService.api;
using TeacherService.Infrastructure;
using TeacherService.Persistence;
using Polly;
using TeacherService.Core;
using TeacherService.Core.VisitCheckers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<EndpointsOptions>()
    .Bind(builder.Configuration.GetSection(EndpointsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<RabbitOptions>()
    .Bind(builder.Configuration.GetSection(RabbitOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<DbOptions>()
    .Bind(builder.Configuration.GetSection(DbOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddTeacherSubject()
    .AddTeacherPersistence()
    .AddTeacherMessaging()
    .AddTeacherClients()
    .AddTeacherDomain();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TeacherDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();
app.Run();