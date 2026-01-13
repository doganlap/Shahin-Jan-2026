using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.FeatureManagement;
using Volo.Abp.AuditLogging;
using Volo.Abp.OpenIddict;
using Volo.Abp.Ldap;
using Volo.Abp.TenantManagement;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;

namespace GrcMvc;

/// <summary>
/// ABP Framework module configuration for Shahin AI GRC System.
/// Integrates 5 FREE ABP modules (MIT License).
/// Commercial modules (Volo.Saas, Volo.Abp.Gdpr, Volo.Payment, Volo.Abp.LanguageManagement)
/// have been replaced with custom implementations to avoid $2,999/year license cost.
/// </summary>
[DependsOn(
    // ABP Core
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),

    // ✅ FREE ABP Modules (MIT License)

    // 1. Feature Management - Feature Toggles ⭐⭐⭐⭐
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementWebModule),

    // 2. Audit Log UI - Browse Audit Logs ⭐⭐⭐
    typeof(AbpAuditLoggingWebModule),

    // 3. OpenIddict - SSO/OAuth2 Provider ⭐⭐⭐
    typeof(AbpOpenIddictDomainModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),

    // 4. LDAP Integration - Active Directory ⭐⭐⭐
    typeof(AbpLdapModule),

    // 5. Tenant Management - Multi-Tenancy ⭐⭐⭐⭐⭐
    typeof(AbpTenantManagementDomainModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpTenantManagementApplicationModule)
)]
public class GrcMvcAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // Pre-configuration for ABP modules
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // ✅ Configure Feature Management (FREE)
        Configure<FeatureManagementOptions>(options =>
        {
            // Features are defined in GrcFeatureDefinitionProvider
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = true;
        });

        // ✅ Configure OpenIddict (FREE)
        Configure<AbpOpenIddictOptions>(options =>
        {
            options.Applications.Add(new OpenIddictApplicationDescriptor
            {
                ClientId = "GrcMvc_Web",
                ClientSecret = configuration["OpenIddict:ClientSecret"] ?? "1q2w3e*",
                DisplayName = "Shahin AI GRC Web Application",
                Type = "web"
            });
        });

        // ✅ Configure LDAP (FREE)
        Configure<AbpLdapOptions>(options =>
        {
            options.ServerHost = configuration["Ldap:ServerHost"] ?? "";
            options.ServerPort = int.Parse(configuration["Ldap:ServerPort"] ?? "389");
            options.BaseDc = configuration["Ldap:BaseDc"] ?? "";
            options.Domain = configuration["Ldap:Domain"] ?? "";
            options.UserName = configuration["Ldap:UserName"] ?? "";
            options.Password = configuration["Ldap:Password"] ?? "";
        });

        // ✅ Configure Tenant Management (FREE)
        // Using existing Tenant table + custom Edition field (Free/Trial/Pro/Enterprise)
        // No need for commercial Volo.Saas module ($2,999/year)
        Configure<AbpTenantManagementOptions>(options =>
        {
            // Tenant isolation enabled at database level via query filters
            options.IsMultiTenant = true;
        });

        // ❌ Commercial module configurations removed (SaaS, GDPR, Payment)
        // Use custom implementations:
        // - Edition management: Use existing TenantService + Edition field
        // - GDPR: Create custom GdprService (see docs/ABP_PACKAGES_LICENSING.md)
        // - Payments: Use Stripe.net SDK directly (see docs/ABP_PACKAGES_LICENSING.md)
        // - Language Management: Build simple CRUD UI (see docs/ABP_PACKAGES_LICENSING.md)
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();

        // ABP modules are automatically initialized
        // Custom initialization logic can go here
    }
}
