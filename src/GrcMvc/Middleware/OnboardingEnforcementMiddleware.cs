using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using System.Threading.Tasks;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Middleware to enforce onboarding completion for all authenticated tenant users
    /// Ensures users cannot access workspace until onboarding wizard is completed
    /// </summary>
    public class OnboardingEnforcementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OnboardingEnforcementMiddleware> _logger;

        public OnboardingEnforcementMiddleware(
            RequestDelegate next,
            ILogger<OnboardingEnforcementMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, GrcDbContext dbContext)
        {
            // Skip enforcement for specific paths (onboarding, account, static files, APIs)
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            if (path.StartsWith("/onboarding") ||
                path.StartsWith("/trial") ||
                path.StartsWith("/account") ||
                path.StartsWith("/api") ||
                path.StartsWith("/health") ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/images") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/favicon") ||
                path == "/" ||
                path == "/home" ||
                path == "/home/index")
            {
                await _next(context);
                return;
            }

            // Skip if user not authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Get TenantId from claims
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                await _next(context);
                return;
            }

            // Check tenant onboarding status
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                await _next(context);
                return;
            }

            // If onboarding not completed, redirect to onboarding wizard
            if (tenant.OnboardingStatus != "COMPLETED")
            {
                _logger.LogInformation(
                    "User {UserId} for tenant {TenantId} redirected to onboarding (status: {Status})",
                    context.User.Identity.Name,
                    tenantId,
                    tenant.OnboardingStatus);

                context.Response.Redirect($"/t/{tenant.TenantSlug}/onboarding/start");
                return;
            }

            // Onboarding completed, continue pipeline
            await _next(context);
        }
    }

    /// <summary>
    /// Extension method for easy middleware registration
    /// </summary>
    public static class OnboardingEnforcementMiddlewareExtensions
    {
        public static IApplicationBuilder UseOnboardingEnforcement(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OnboardingEnforcementMiddleware>();
        }
    }
}
