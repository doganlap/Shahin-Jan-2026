using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Account.Web;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using GrcMvc.Data;

namespace GrcMvc
{
    /// <summary>
    /// ABP Module for GRC MVC application
    /// Integrates ABP Framework tenant management with existing custom implementation
    /// </summary>
    [DependsOn(
        // Autofac for DI
        typeof(AbpAutofacModule),

        // ASP.NET Core MVC
        typeof(AbpAspNetCoreMvcModule),

        // Entity Framework Core
        typeof(AbpEntityFrameworkCorePostgreSqlModule),

        // Tenant Management
        typeof(AbpTenantManagementApplicationModule),
        typeof(AbpTenantManagementWebModule),
        typeof(AbpTenantManagementEntityFrameworkCoreModule),

        // Identity
        typeof(AbpIdentityApplicationModule),
        typeof(AbpIdentityAspNetCoreModule),
        typeof(AbpIdentityEntityFrameworkCoreModule),

        // Account (Login/Register)
        typeof(AbpAccountWebModule),

        // Supporting Modules
        typeof(AbpPermissionManagementEntityFrameworkCoreModule),
        typeof(AbpSettingManagementEntityFrameworkCoreModule),
        typeof(AbpAuditLoggingEntityFrameworkCoreModule),
        typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
    public class GrcMvcWebModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostingEnvironment = context.Services.GetHostingEnvironment();

            // ═══════════════════════════════════════════════════
            // MULTI-TENANCY - REQUIRED
            // ═══════════════════════════════════════════════════
            Configure<AbpMultiTenancyOptions>(options =>
            {
                options.IsEnabled = true;
            });

            // ═══════════════════════════════════════════════════
            // TENANT RESOLVER - HOW ABP DETECTS TENANT
            // ═══════════════════════════════════════════════════
            Configure<AbpTenantResolveOptions>(options =>
            {
                // Order matters - first match wins
                options.TenantResolvers.Clear();
                options.TenantResolvers.Add(new CurrentUserTenantResolveContributor()); // From logged-in user
                options.TenantResolvers.Add(new CookieTenantResolveContributor());      // From cookie
                options.TenantResolvers.Add(new QueryStringTenantResolveContributor()); // From ?__tenant=xxx
                options.TenantResolvers.Add(new HeaderTenantResolveContributor());      // From header
                // Subdomain resolver can be added if needed:
                // options.TenantResolvers.Add(new DomainTenantResolveContributor("{0}.shahin-ai.com"));
            });

            // ═══════════════════════════════════════════════════
            // EF CORE - Configure DbContext
            // ═══════════════════════════════════════════════════
            Configure<AbpDbContextOptions>(options =>
            {
                options.UseNpgsql();
            });

            // ═══════════════════════════════════════════════════
            // IDENTITY OPTIONS - Relaxed for trials
            // ═══════════════════════════════════════════════════
            Configure<IdentityOptions>(options =>
            {
                // Relaxed password policy for easier trials (adjust for production)
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Lockout
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            // ABP multi-tenancy middleware
            app.UseMultiTenancy();

            // ABP auditing
            app.UseAuditing();
        }
    }
}
