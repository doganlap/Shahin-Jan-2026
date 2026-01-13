using GrcMvc.Application.Features;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Example controller demonstrating how to use feature checks.
/// Shows various patterns for implementing feature-gated functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeatureCheckExampleController : ControllerBase
{
    private readonly IFeatureCheckService _featureCheckService;
    private readonly ILogger<FeatureCheckExampleController> _logger;

    public FeatureCheckExampleController(
        IFeatureCheckService featureCheckService,
        ILogger<FeatureCheckExampleController> logger)
    {
        _featureCheckService = featureCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Example: Check if advanced reporting is available before generating report.
    /// </summary>
    [HttpGet("advanced-report")]
    public async Task<IActionResult> GetAdvancedReport()
    {
        // Check feature availability
        if (!await _featureCheckService.IsAdvancedReportingEnabledAsync())
        {
            return BadRequest(new
            {
                error = "FeatureNotAvailable",
                message = "Advanced Reporting is not available in your current subscription plan.",
                upgradeUrl = "/Subscription/Upgrade"
            });
        }

        // Feature is available, proceed with report generation
        var report = new { /* report data */ };
        return Ok(report);
    }

    /// <summary>
    /// Example: Check AI agent quota before processing query.
    /// </summary>
    [HttpPost("ai-query")]
    public async Task<IActionResult> ProcessAIQuery([FromBody] string query)
    {
        // Check if AI agents feature is enabled
        if (!await _featureCheckService.IsAIAgentsEnabledAsync())
        {
            return BadRequest(new
            {
                error = "FeatureNotAvailable",
                message = "AI Agents are not available in your current subscription plan."
            });
        }

        // Check query limit (would need to track usage in database)
        var queryLimit = await _featureCheckService.GetAIAgentQueryLimitAsync();
        // var currentUsage = await GetCurrentMonthQueryCount(); // Implement this
        // if (currentUsage >= queryLimit) { return 429 Too Many Requests }

        // Process AI query
        return Ok(new { response = "AI response here", remainingQueries = queryLimit });
    }

    /// <summary>
    /// Example: Check workspace limit before creating new workspace.
    /// </summary>
    [HttpPost("workspace")]
    public async Task<IActionResult> CreateWorkspace([FromBody] string name)
    {
        // Get current workspace count for tenant
        // var currentWorkspaceCount = await GetTenantWorkspaceCount();

        // Check if limit reached
        var workspaceLimit = await _featureCheckService.GetWorkspaceLimitAsync();
        // if (currentWorkspaceCount >= workspaceLimit)
        // {
        //     return BadRequest($"Workspace limit reached. Your plan allows {workspaceLimit} workspaces.");
        // }

        // Create workspace
        return Ok(new { message = "Workspace created", name });
    }

    /// <summary>
    /// Example: Check user limit before inviting user.
    /// </summary>
    [HttpPost("invite-user")]
    public async Task<IActionResult> InviteUser([FromBody] string email)
    {
        // Get current user count for tenant
        // var currentUserCount = await GetTenantUserCount();

        // Check if limit reached using helper method
        var userLimit = await _featureCheckService.GetUserLimitAsync();
        // var isLimitReached = await _featureCheckService.IsLimitReachedAsync(GrcFeatures.UserLimit, currentUserCount);
        // if (isLimitReached)
        // {
        //     return BadRequest($"User limit reached. Your plan allows {userLimit} users.");
        // }

        // Send invitation
        return Ok(new { message = "Invitation sent", email });
    }

    /// <summary>
    /// Example: Get feature status for current tenant (useful for UI).
    /// </summary>
    [HttpGet("feature-status")]
    public async Task<IActionResult> GetFeatureStatus()
    {
        var status = new
        {
            advancedReporting = await _featureCheckService.IsAdvancedReportingEnabledAsync(),
            aiAgents = await _featureCheckService.IsAIAgentsEnabledAsync(),
            aiAgentQueryLimit = await _featureCheckService.GetAIAgentQueryLimitAsync(),
            workflowAutomation = await _featureCheckService.IsWorkflowAutomationEnabledAsync(),
            workspaceLimit = await _featureCheckService.GetWorkspaceLimitAsync(),
            userLimit = await _featureCheckService.GetUserLimitAsync(),
            ssoLdap = await _featureCheckService.IsSsoLdapEnabledAsync(),
            apiAccess = await _featureCheckService.IsApiAccessEnabledAsync()
        };

        return Ok(status);
    }

    /// <summary>
    /// Example: Generic feature check endpoint.
    /// </summary>
    [HttpGet("check/{featureName}")]
    public async Task<IActionResult> CheckFeature(string featureName)
    {
        try
        {
            var isEnabled = await _featureCheckService.IsEnabledAsync(featureName);
            var value = await _featureCheckService.GetValueAsync(featureName);

            return Ok(new
            {
                feature = featureName,
                enabled = isEnabled,
                value = value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature: {FeatureName}", featureName);
            return StatusCode(500, new { error = "Failed to check feature" });
        }
    }
}
