using Microsoft.EntityFrameworkCore;
using SessionService.Persistence;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("SessionDb")
    ?? throw new InvalidOperationException("ConnectionStrings:SessionDb отсутствует");

builder.Services.AddDbContext<SessionDbContext>(o => o.UseNpgsql(conn));
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SessionDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();
app.Run();