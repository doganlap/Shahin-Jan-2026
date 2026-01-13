using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Saas.Editions;
using GrcMvc.Application.Features;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds SaaS editions (Free, Professional, Enterprise) with feature configurations.
/// Defines subscription tiers and their capabilities.
/// </summary>
public class GrcEditionDataSeeder : ITransientDependency
{
    private readonly IEditionRepository _editionRepository;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<GrcEditionDataSeeder> _logger;

    public GrcEditionDataSeeder(
        IEditionRepository editionRepository,
        IFeatureManager featureManager,
        ILogger<GrcEditionDataSeeder> logger)
    {
        _editionRepository = editionRepository;
        _featureManager = featureManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Seeding GRC Editions...");

        // 1. Free Edition
        await SeedFreeEditionAsync();

        // 2. Professional Edition
        await SeedProfessionalEditionAsync();

        // 3. Enterprise Edition
        await SeedEnterpriseEditionAsync();

        _logger.LogInformation("GRC Editions seeded successfully!");
    }

    private async Task SeedFreeEditionAsync()
    {
        var edition = await _editionRepository.FindByDisplayNameAsync("Free");
        if (edition == null)
        {
            edition = new Edition(Guid.NewGuid(), "Free")
            {
                DisplayName = "Free",
            };
            await _editionRepository.InsertAsync(edition);
            _logger.LogInformation("Created Free edition");
        }

        // Configure features for Free edition
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AdvancedReporting, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AIAgents, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AIAgentQueryLimit, "10");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.ComplianceFrameworks, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.FrameworkLimit, "3");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.RiskAnalytics, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.WorkflowAutomation, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.WorkspaceLimit, "1");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.UserLimit, "5");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.SsoLdap, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.CustomBranding, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.PrioritySupport, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.ApiAccess, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.CustomIntegrations, "false");

        _logger.LogInformation("Configured Free edition features");
    }

    private async Task SeedProfessionalEditionAsync()
    {
        var edition = await _editionRepository.FindByDisplayNameAsync("Professional");
        if (edition == null)
        {
            edition = new Edition(Guid.NewGuid(), "Professional")
            {
                DisplayName = "Professional",
            };
            await _editionRepository.InsertAsync(edition);
            _logger.LogInformation("Created Professional edition");
        }

        // Configure features for Professional edition
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AdvancedReporting, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AIAgents, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AIAgentQueryLimit, "500");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.ComplianceFrameworks, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.FrameworkLimit, "20");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.RiskAnalytics, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.WorkflowAutomation, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.WorkspaceLimit, "5");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.UserLimit, "50");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.SsoLdap, "false");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.CustomBranding, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.PrioritySupport, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.ApiAccess, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.CustomIntegrations, "false");

        _logger.LogInformation("Configured Professional edition features");
    }

    private async Task SeedEnterpriseEditionAsync()
    {
        var edition = await _editionRepository.FindByDisplayNameAsync("Enterprise");
        if (edition == null)
        {
            edition = new Edition(Guid.NewGuid(), "Enterprise")
            {
                DisplayName = "Enterprise",
            };
            await _editionRepository.InsertAsync(edition);
            _logger.LogInformation("Created Enterprise edition");
        }

        // Configure features for Enterprise edition (unlimited)
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AdvancedReporting, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AIAgents, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.AIAgentQueryLimit, "10000");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.ComplianceFrameworks, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.FrameworkLimit, "100");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.RiskAnalytics, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.WorkflowAutomation, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.WorkspaceLimit, "1000");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.UserLimit, "10000");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.SsoLdap, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.CustomBranding, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.PrioritySupport, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.ApiAccess, "true");
        await _featureManager.SetForEditionAsync(edition.Id, GrcFeatures.CustomIntegrations, "true");

        _logger.LogInformation("Configured Enterprise edition features");
    }
}
