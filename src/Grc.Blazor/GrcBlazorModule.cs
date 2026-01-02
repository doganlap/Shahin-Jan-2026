using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;
using Grc.Blazor.Services;
using Grc.Blazor.Menus;
using Grc.Application.Contracts;
using Grc.Application;

namespace Grc.Blazor;

[DependsOn(
    typeof(GrcApplicationContractsModule),
    typeof(GrcApplicationModule)
)]
public class GrcBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IMenuContributor, GrcMenuContributor>();
        context.Services.AddScoped<ErrorToastService>();
    }
}
