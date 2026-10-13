using HistoryService;
using HistoryService.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("HistoryDb")
    ?? throw new InvalidOperationException("ConnectionStrings:HistoryDb отсутствует");

builder.Services.AddDbContext<HistoryDbContext>(o => o.UseNpgsql(conn));
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<Service>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RecordAskedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:User"] ?? "dev");
            h.Password(builder.Configuration["RabbitMq:Pass"] ?? "dev");
        });

        cfg.ReceiveEndpoint("asked-recorded", e =>
        {
            e.UseMessageRetry(r =>
            {
                r.Immediate(3);
                r.Interval(2, TimeSpan.FromSeconds(5));
                r.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromSeconds(10),
                    maxInterval: TimeSpan.FromSeconds(60),
                    intervalDelta: TimeSpan.FromSeconds(5));
            });

            e.PrefetchCount = 16;

            e.ConfigureConsumer<RecordAskedConsumer>(ctx);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
