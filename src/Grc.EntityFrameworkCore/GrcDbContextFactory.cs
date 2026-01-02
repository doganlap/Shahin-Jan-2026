using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Grc.EntityFrameworkCore;

/// <summary>
/// Design-time factory for EF Core migrations
/// </summary>
public class GrcDbContextFactory : IDesignTimeDbContextFactory<GrcDbContext>
{
    public GrcDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<GrcDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new GrcDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = Directory.GetCurrentDirectory();
        
        // Look for appsettings.json in the solution root
        var solutionRoot = Path.GetFullPath(Path.Combine(basePath, "..", ".."));
        var appSettingsPath = Path.Combine(solutionRoot, "appsettings.json");
        
        if (!File.Exists(appSettingsPath))
        {
            // Fallback to current directory
            appSettingsPath = Path.Combine(basePath, "appsettings.json");
        }
        
        if (!File.Exists(appSettingsPath))
        {
            throw new FileNotFoundException($"Configuration file not found at {appSettingsPath}");
        }

        return new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(appSettingsPath)!)
            .AddJsonFile(Path.GetFileName(appSettingsPath), optional: false)
            .Build();
    }
}
