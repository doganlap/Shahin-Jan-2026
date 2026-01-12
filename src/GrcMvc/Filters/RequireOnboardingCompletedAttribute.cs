using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using System.Security.Claims;

namespace GrcMvc.Filters
{
    /// <summary>
    /// Authorization filter to ensure user has completed onboarding wizard
    /// before accessing protected resources (workspace, dashboard, etc.)
    /// </summary>
    public class RequireOnboardingCompletedAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Skip check for onboarding controller itself and public pages
            var controller = context.Controller.GetType().Name;
            if (controller == "OnboardingController" ||
                controller == "AccountController" ||
                controller == "TrialController" ||
                controller == "HomeController")
            {
                await next();
                return;
            }

            var httpContext = context.HttpContext;

            // Check if user is authenticated
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                await next();
                return;
            }

            // Get TenantId from claims
            var tenantIdClaim = httpContext.User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                await next();
                return;
            }

            // Check if tenant has completed onboarding
            var dbContext = httpContext.RequestServices.GetRequiredService<GrcDbContext>();
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                await next();
                return;
            }

            // If onboarding not completed, redirect to onboarding wizard
            if (tenant.OnboardingStatus != "COMPLETED" &&
                !string.IsNullOrEmpty(tenant.TenantSlug))
            {
                context.Result = new RedirectToActionResult(
                    actionName: "Start",
                    controllerName: "Onboarding",
                    routeValues: new { tenantSlug = tenant.TenantSlug });
                return;
            }

            // Onboarding completed, continue to action
            await next();
        }
    }
}
