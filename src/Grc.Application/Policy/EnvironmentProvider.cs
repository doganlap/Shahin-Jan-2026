using Microsoft.Extensions.Configuration;

namespace Grc.Application.Policy;

public class EnvironmentProvider : IEnvironmentProvider
{
    private readonly IConfiguration _configuration;

    public EnvironmentProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetEnvironment()
    {
        // Try configuration first, then environment variable, then default
        return _configuration["Environment:Name"]?.ToLowerInvariant()
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant()
            ?? "dev";
    }
}
