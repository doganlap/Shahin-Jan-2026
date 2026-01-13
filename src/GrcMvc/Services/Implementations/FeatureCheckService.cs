using GrcMvc.Application.Features;
using GrcMvc.Services.Interfaces;
using Volo.Abp.Features;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Implementation of feature checking service using ABP's IFeatureChecker.
/// Provides convenient methods for checking GRC-specific features.
/// </summary>
public class FeatureCheckService : IFeatureCheckService
{
    private readonly IFeatureChecker _featureChecker;

    public FeatureCheckService(IFeatureChecker featureChecker)
    {
        _featureChecker = featureChecker;
    }

    public async Task<bool> IsEnabledAsync(string featureName)
    {
        return await _featureChecker.IsEnabledAsync(featureName);
    }

    public async Task<string?> GetValueAsync(string featureName)
    {
        return await _featureChecker.GetOrNullAsync(featureName);
    }

    public async Task<int?> GetIntValueAsync(string featureName)
    {
        var value = await GetValueAsync(featureName);
        if (string.IsNullOrEmpty(value))
            return null;

        if (int.TryParse(value, out var intValue))
            return intValue;

        return null;
    }

    public async Task<bool> IsAdvancedReportingEnabledAsync()
    {
        return await IsEnabledAsync(GrcFeatures.AdvancedReporting);
    }

    public async Task<bool> IsAIAgentsEnabledAsync()
    {
        return await IsEnabledAsync(GrcFeatures.AIAgents);
    }

    public async Task<int> GetAIAgentQueryLimitAsync()
    {
        var limit = await GetIntValueAsync(GrcFeatures.AIAgentQueryLimit);
        return limit ?? 10; // Default to 10 if not set
    }

    public async Task<bool> IsWorkflowAutomationEnabledAsync()
    {
        return await IsEnabledAsync(GrcFeatures.WorkflowAutomation);
    }

    public async Task<int> GetWorkspaceLimitAsync()
    {
        var limit = await GetIntValueAsync(GrcFeatures.WorkspaceLimit);
        return limit ?? 1; // Default to 1 if not set
    }

    public async Task<int> GetUserLimitAsync()
    {
        var limit = await GetIntValueAsync(GrcFeatures.UserLimit);
        return limit ?? 5; // Default to 5 if not set
    }

    public async Task<bool> IsSsoLdapEnabledAsync()
    {
        return await IsEnabledAsync(GrcFeatures.SsoLdap);
    }

    public async Task<bool> IsApiAccessEnabledAsync()
    {
        return await IsEnabledAsync(GrcFeatures.ApiAccess);
    }

    public async Task<bool> IsLimitReachedAsync(string featureName, int currentCount)
    {
        var limit = await GetIntValueAsync(featureName);
        if (limit == null)
            return false; // No limit set

        return currentCount >= limit.Value;
    }
}
