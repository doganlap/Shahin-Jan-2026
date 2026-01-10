using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Middleware to check if owner setup is required and redirect to setup page
    /// This runs before authentication to allow owner setup when no owner exists
    /// </summary>
    public class OwnerSetupMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OwnerSetupMiddleware> _logger;
        private static bool? _ownerExistsCache = null;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

        public OwnerSetupMiddleware(
            RequestDelegate next,
            ILogger<OwnerSetupMiddleware> logger)
        {
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "K", location = "OwnerSetupMiddleware.cs:21", message = "OwnerSetupMiddleware constructor", data = new { nextExists = next != null, loggerExists = logger != null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            _next = next;
            _logger = logger;
            _logger.LogDebug("OwnerSetupMiddleware initialized");
        }

        public async Task InvokeAsync(HttpContext context, IOwnerSetupService ownerSetupService)
        {
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "K", location = "OwnerSetupMiddleware.cs:32", message = "InvokeAsync entry", data = new { path = context.Request.Path.Value, method = context.Request.Method, ownerSetupServiceExists = ownerSetupService != null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch (Exception logEx) { _logger.LogError(logEx, "Failed to write debug log at InvokeAsync entry"); }
            // #endregion
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "K", location = "OwnerSetupMiddleware.cs:37", message = "After path lowercasing", data = new { path = path, originalPath = context.Request.Path.Value, pathLength = path.Length, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch (Exception logEx) { _logger.LogError(logEx, "Failed to write debug log after path lowercasing"); }
            // #endregion

            _logger.LogDebug("OwnerSetupMiddleware processing path: {Path}", path);

            // Skip middleware for:
            // - OwnerSetup controller (to avoid redirect loop) - case insensitive
            // - Landing page routes (public marketing pages) - should be accessible without owner
            // - Static files
            // - API endpoints
            // - Health checks
            // Path is already lowercased above, so simple string comparison works
            bool shouldSkip = path.StartsWith("/ownersetup") ||
                path == "/" ||
                path == "/home" ||
                path.StartsWith("/landing/") ||
                path.StartsWith("/pricing") ||
                path.StartsWith("/features") ||
                path.StartsWith("/about") ||
                path.StartsWith("/contact") ||
                path.StartsWith("/case-studies") ||
                path.StartsWith("/grc-free-trial") ||
                path.StartsWith("/best-grc-software") ||
                path.StartsWith("/why-our-grc") ||
                path.StartsWith("/grc-for-") ||
                path.StartsWith("/api/") ||
                path.StartsWith("/_") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/lib/") ||
                path.StartsWith("/images/") ||
                path.StartsWith("/health") ||
                path == "/favicon.ico";

            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "K", location = "OwnerSetupMiddleware.cs:65", message = "Path skip check result", data = new { path = path, shouldSkip = shouldSkip, pathEqualsSlash = path == "/", pathStartsWithOwnersetup = path.StartsWith("/ownersetup"), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch (Exception logEx) { _logger.LogError(logEx, "Failed to write debug log for skip check"); }
            // #endregion

            if (shouldSkip)
            {
                _logger.LogDebug("OwnerSetupMiddleware skipping path: {Path}", path);
                await _next(context);
                return;
            }

            try
            {
                // Check cache first (to avoid DB query on every request)
                bool ownerExists;
                if (_ownerExistsCache.HasValue && DateTime.UtcNow < _cacheExpiry)
                {
                    ownerExists = _ownerExistsCache.Value;
                    _logger.LogDebug("Owner existence from cache: {OwnerExists}", ownerExists);
                }
                else
                {
                    _logger.LogDebug("Checking owner existence in database for path: {Path}", path);
                    ownerExists = await ownerSetupService.OwnerExistsAsync();
                    _ownerExistsCache = ownerExists;
                    _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                    _logger.LogDebug("Owner existence check result: {OwnerExists}", ownerExists);
                }

                // If no owner exists and user is not already on setup page, redirect
                if (!ownerExists && !path.StartsWith("/account/login"))
                {
                    _logger.LogInformation("Redirecting to owner setup page. Path: {Path}, OwnerExists: {OwnerExists}", path, ownerExists);
                    context.Response.Redirect("/OwnerSetup");
                    return;
                }

                // Clear cache if owner was just created (to allow immediate access)
                if (ownerExists && _ownerExistsCache.HasValue && !_ownerExistsCache.Value)
                {
                    _ownerExistsCache = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OwnerSetupMiddleware for path: {Path}", path);
                // On error, continue to next middleware (don't block the app)
            }

            await _next(context);
        }
    }
}
