namespace HistoryService.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class HistoryDbContextFactory : IDesignTimeDbContextFactory<HistoryDbContext>
{
    public HistoryDbContext CreateDbContext(string[] args)
    {
        const string conn = "Host=localhost;Port=5432;Database=history;Username=dev;Password=dev";
        var options = new DbContextOptionsBuilder<HistoryDbContext>()
            .UseNpgsql(conn)
            .Options;
        return new HistoryDbContext(options);
    }
}
