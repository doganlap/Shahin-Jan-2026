namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for checking feature availability for current tenant.
/// Wraps ABP's IFeatureChecker with GRC-specific feature constants.
/// </summary>
public interface IFeatureCheckService
{
    /// <summary>
    /// Checks if a feature is enabled for the current tenant.
    /// </summary>
    /// <param name="featureName">Feature name (use GrcFeatures constants)</param>
    /// <returns>True if feature is enabled, false otherwise</returns>
    Task<bool> IsEnabledAsync(string featureName);

    /// <summary>
    /// Gets the value of a feature for the current tenant.
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>Feature value (for limits/quotas)</returns>
    Task<string?> GetValueAsync(string featureName);

    /// <summary>
    /// Gets an integer feature value (for limits like UserLimit, WorkspaceLimit).
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>Integer value, or null if not set</returns>
    Task<int?> GetIntValueAsync(string featureName);

    /// <summary>
    /// Checks if Advanced Reporting feature is enabled.
    /// </summary>
    Task<bool> IsAdvancedReportingEnabledAsync();

    /// <summary>
    /// Checks if AI Agents feature is enabled.
    /// </summary>
    Task<bool> IsAIAgentsEnabledAsync();

    /// <summary>
    /// Gets the AI agent query limit for current tenant.
    /// </summary>
    Task<int> GetAIAgentQueryLimitAsync();

    /// <summary>
    /// Checks if Workflow Automation is enabled.
    /// </summary>
    Task<bool> IsWorkflowAutomationEnabledAsync();

    /// <summary>
    /// Gets the workspace limit for current tenant.
    /// </summary>
    Task<int> GetWorkspaceLimitAsync();

    /// <summary>
    /// Gets the user limit for current tenant.
    /// </summary>
    Task<int> GetUserLimitAsync();

    /// <summary>
    /// Checks if SSO/LDAP is enabled.
    /// </summary>
    Task<bool> IsSsoLdapEnabledAsync();

    /// <summary>
    /// Checks if API Access is enabled.
    /// </summary>
    Task<bool> IsApiAccessEnabledAsync();

    /// <summary>
    /// Checks if feature limit has been reached.
    /// </summary>
    /// <param name="featureName">Feature name (e.g., GrcFeatures.UserLimit)</param>
    /// <param name="currentCount">Current count (e.g., number of users)</param>
    /// <returns>True if limit reached, false otherwise</returns>
    Task<bool> IsLimitReachedAsync(string featureName, int currentCount);
}
