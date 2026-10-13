namespace SessionService.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class SessionDbContextFactory : IDesignTimeDbContextFactory<SessionDbContext>
{
    public SessionDbContext CreateDbContext(string[] args)
    {
        const string conn = "Host=localhost;Port=5432;Database=sessions;Username=dev;Password=dev";
        var options = new DbContextOptionsBuilder<SessionDbContext>()
            .UseNpgsql(conn)
            .Options;
        return new SessionDbContext(options);
    }
}
