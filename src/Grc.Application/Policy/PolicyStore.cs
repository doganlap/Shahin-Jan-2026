using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Grc.Application.Policy.PolicyModels;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PolicyDoc = Grc.Application.Policy.PolicyModels.PolicyDocument;

namespace Grc.Application.Policy;

public interface IPolicyStore
{
    Task<PolicyDoc?> LoadPolicyAsync(string policyPath, CancellationToken ct = default);
    PolicyDoc? GetCachedPolicy(string policyName);
    void InvalidateCache(string policyName);
}

public class PolicyStore : IPolicyStore
{
    private readonly ConcurrentDictionary<string, PolicyDoc> _cache = new();
    private readonly ILogger<PolicyStore> _logger;
    private readonly string _policiesBasePath;

    public PolicyStore(ILogger<PolicyStore> logger, string? policiesBasePath = null)
    {
        _logger = logger;
        _policiesBasePath = policiesBasePath ?? Path.Combine(AppContext.BaseDirectory, "etc", "policies");
    }

    public async Task<PolicyDoc?> LoadPolicyAsync(string policyPath, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(policyPath) 
            ? policyPath 
            : Path.Combine(_policiesBasePath, policyPath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Policy file not found: {Path}", fullPath);
            return null;
        }

        var policyName = Path.GetFileNameWithoutExtension(policyPath);
        if (_cache.TryGetValue(policyName, out var cached))
        {
            return cached;
        }

        try
        {
            var yamlContent = await File.ReadAllTextAsync(fullPath, ct);
            
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var policy = deserializer.Deserialize<PolicyDoc>(yamlContent);
            
            if (policy != null)
            {
                _cache.TryAdd(policyName, policy);
                _logger.LogInformation("Loaded policy: {PolicyName} from {Path}", policyName, fullPath);
            }

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load policy from {Path}", fullPath);
            return null;
        }
    }

    public PolicyDoc? GetCachedPolicy(string policyName)
    {
        return _cache.TryGetValue(policyName, out var policy) ? policy : null;
    }

    public void InvalidateCache(string policyName)
    {
        _cache.TryRemove(policyName, out _);
        _logger.LogInformation("Invalidated cache for policy: {PolicyName}", policyName);
    }
}
