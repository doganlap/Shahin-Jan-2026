using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Modularity;
using Grc.Application.Policy;
using Grc.Application.Admin;

namespace Grc.Application;

public class GrcApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // Register policy services
        services.AddSingleton<IPolicyStore>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<PolicyStore>>();
            var policiesPath = Path.Combine(AppContext.BaseDirectory, "etc", "policies");
            return new PolicyStore(logger, policiesPath);
        });

        services.AddScoped<IPolicyEnforcer, PolicyEnforcer>();
        services.AddScoped<IPolicyAuditLogger, PolicyAuditLogger>();

        // Register supporting services for policy engine
        services.AddScoped<IEnvironmentProvider, EnvironmentProvider>();
        services.AddScoped<IRoleResolver, RoleResolver>();

        // Register AutoMapper
        services.AddAutoMapper(typeof(AdminApplicationAutoMapperProfile));
        services.AddAutoMapper(typeof(GrcApplicationAutoMapperProfile));

        // Register Role Profile Services
        services.AddTransient<Grc.Application.Contracts.Roles.IRoleProfileAppService, Roles.RoleProfileAppService>();
        services.AddTransient<Grc.Application.Contracts.Roles.IRoleProfileIntegrationService, Roles.RoleProfileIntegrationService>();
    }
}
