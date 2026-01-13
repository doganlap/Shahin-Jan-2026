using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace GrcMvc.Application.Features;

/// <summary>
/// Defines all GRC features that can be enabled/disabled per tenant or edition.
/// Features control access to premium functionality based on subscription tier.
/// </summary>
public class GrcFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        // Feature Group: GRC Core
        var grcGroup = context.AddGroup("Grc", L("Feature:GRC"));

        // 1. Advanced Reporting
        grcGroup.AddFeature(
            GrcFeatures.AdvancedReporting,
            defaultValue: "false",
            displayName: L("Feature:AdvancedReporting"),
            description: L("Feature:AdvancedReporting:Description"),
            valueType: new ToggleStringValueType()
        );

        // 2. AI Agents
        var aiAgents = grcGroup.AddFeature(
            GrcFeatures.AIAgents,
            defaultValue: "false",
            displayName: L("Feature:AIAgents"),
            description: L("Feature:AIAgents:Description"),
            valueType: new ToggleStringValueType()
        );

        // 2.1 AI Agent Query Limit (child feature)
        aiAgents.CreateChild(
            GrcFeatures.AIAgentQueryLimit,
            defaultValue: "10",
            displayName: L("Feature:AIAgentQueryLimit"),
            description: L("Feature:AIAgentQueryLimit:Description"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(0, 10000))
        );

        // 3. Compliance Frameworks
        var frameworks = grcGroup.AddFeature(
            GrcFeatures.ComplianceFrameworks,
            defaultValue: "false",
            displayName: L("Feature:ComplianceFrameworks"),
            description: L("Feature:ComplianceFrameworks:Description"),
            valueType: new ToggleStringValueType()
        );

        // 3.1 Framework Limit
        frameworks.CreateChild(
            GrcFeatures.FrameworkLimit,
            defaultValue: "3",
            displayName: L("Feature:FrameworkLimit"),
            description: L("Feature:FrameworkLimit:Description"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(1, 100))
        );

        // 4. Risk Analytics
        grcGroup.AddFeature(
            GrcFeatures.RiskAnalytics,
            defaultValue: "false",
            displayName: L("Feature:RiskAnalytics"),
            description: L("Feature:RiskAnalytics:Description"),
            valueType: new ToggleStringValueType()
        );

        // 5. Workflow Automation
        grcGroup.AddFeature(
            GrcFeatures.WorkflowAutomation,
            defaultValue: "false",
            displayName: L("Feature:WorkflowAutomation"),
            description: L("Feature:WorkflowAutomation:Description"),
            valueType: new ToggleStringValueType()
        );

        // 6. Workspace Limit
        grcGroup.AddFeature(
            GrcFeatures.WorkspaceLimit,
            defaultValue: "1",
            displayName: L("Feature:WorkspaceLimit"),
            description: L("Feature:WorkspaceLimit:Description"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(1, 1000))
        );

        // 7. User Limit
        grcGroup.AddFeature(
            GrcFeatures.UserLimit,
            defaultValue: "5",
            displayName: L("Feature:UserLimit"),
            description: L("Feature:UserLimit:Description"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(1, 10000))
        );

        // 8. SSO/LDAP
        grcGroup.AddFeature(
            GrcFeatures.SsoLdap,
            defaultValue: "false",
            displayName: L("Feature:SsoLdap"),
            description: L("Feature:SsoLdap:Description"),
            valueType: new ToggleStringValueType()
        );

        // 9. Custom Branding
        grcGroup.AddFeature(
            GrcFeatures.CustomBranding,
            defaultValue: "false",
            displayName: L("Feature:CustomBranding"),
            description: L("Feature:CustomBranding:Description"),
            valueType: new ToggleStringValueType()
        );

        // 10. Priority Support
        grcGroup.AddFeature(
            GrcFeatures.PrioritySupport,
            defaultValue: "false",
            displayName: L("Feature:PrioritySupport"),
            description: L("Feature:PrioritySupport:Description"),
            valueType: new ToggleStringValueType()
        );

        // 11. API Access
        grcGroup.AddFeature(
            GrcFeatures.ApiAccess,
            defaultValue: "false",
            displayName: L("Feature:ApiAccess"),
            description: L("Feature:ApiAccess:Description"),
            valueType: new ToggleStringValueType()
        );

        // 12. Custom Integrations
        grcGroup.AddFeature(
            GrcFeatures.CustomIntegrations,
            defaultValue: "false",
            displayName: L("Feature:CustomIntegrations"),
            description: L("Feature:CustomIntegrations:Description"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GrcMvc.Resources.SharedResource>(name);
    }
}

/// <summary>
/// Feature name constants for GRC system.
/// Use these constants when checking feature availability.
/// </summary>
public static class GrcFeatures
{
    public const string GroupName = "Grc";

    // Core Features
    public const string AdvancedReporting = GroupName + ".AdvancedReporting";
    public const string AIAgents = GroupName + ".AIAgents";
    public const string AIAgentQueryLimit = AIAgents + ".QueryLimit";
    public const string ComplianceFrameworks = GroupName + ".ComplianceFrameworks";
    public const string FrameworkLimit = ComplianceFrameworks + ".Limit";
    public const string RiskAnalytics = GroupName + ".RiskAnalytics";
    public const string WorkflowAutomation = GroupName + ".WorkflowAutomation";

    // Limits
    public const string WorkspaceLimit = GroupName + ".WorkspaceLimit";
    public const string UserLimit = GroupName + ".UserLimit";

    // Enterprise Features
    public const string SsoLdap = GroupName + ".SsoLdap";
    public const string CustomBranding = GroupName + ".CustomBranding";
    public const string PrioritySupport = GroupName + ".PrioritySupport";
    public const string ApiAccess = GroupName + ".ApiAccess";
    public const string CustomIntegrations = GroupName + ".CustomIntegrations";
}
