namespace TeacherService.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class TeacherDbContextFactory : IDesignTimeDbContextFactory<TeacherDbContext>
{
    public TeacherDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TeacherDbContext>()
            .UseSqlite("Data Source=teacher.db")
            .Options;
        return new TeacherDbContext(options);
    }
}
