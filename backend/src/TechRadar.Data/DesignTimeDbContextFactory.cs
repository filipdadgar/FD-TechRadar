using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechRadar.Data;

/// <summary>
/// Used by EF Core CLI tools (dotnet ef migrations add) at design time.
/// A real PostgreSQL instance is not required for migration generation.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TechRadarDbContext>
{
    public TechRadarDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TechRadarDbContext>()
            .UseNpgsql("Host=localhost;Database=techradar_design;Username=techradar;Password=design")
            .Options;
        return new TechRadarDbContext(options);
    }
}
