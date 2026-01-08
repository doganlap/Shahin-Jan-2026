using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// MVC Controller for guided onboarding UI
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class OnboardingUiController : Controller
    {
        /// <summary>
        /// Guided onboarding welcome page with microcopy
        /// </summary>
        [HttpGet("guided")]
        public IActionResult GuidedWelcome()
        {
            return View("GuidedWelcome");
        }
    }

    /// <summary>
    /// API Controller for onboarding endpoints
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/onboarding")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class OnboardingApiController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IOnboardingService _onboardingService;
        private readonly ISmartOnboardingService _smartOnboardingService;
        private readonly IRulesEngineService _rulesEngine;
        private readonly ILogger<OnboardingApiController> _logger;

        public OnboardingApiController(
            ITenantService tenantService,
            IOnboardingService onboardingService,
            ISmartOnboardingService smartOnboardingService,
            IRulesEngineService rulesEngine,
            ILogger<OnboardingApiController> logger)
        {
            _tenantService = tenantService;
            _onboardingService = onboardingService;
            _smartOnboardingService = smartOnboardingService;
            _rulesEngine = rulesEngine;
            _logger = logger;
        }

        /// <summary>
        /// Create a new tenant (organization signup).
        /// Sends activation email to admin.
        /// </summary>
        /// <param name="request">Organization name, admin email, and slug</param>
        /// <returns>Tenant ID and activation URL</returns>
        [HttpPost("signup")]
        public async Task<IActionResult> SignupAsync([FromBody] CreateTenantDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrganizationName) ||
                    string.IsNullOrWhiteSpace(request.AdminEmail) ||
                    string.IsNullOrWhiteSpace(request.TenantSlug))
                {
                    return BadRequest("Organization name, admin email, and slug are required.");
                }

                var tenant = await _tenantService.CreateTenantAsync(
                    request.OrganizationName,
                    request.AdminEmail,
                    request.TenantSlug);

                return Ok(new
                {
                    tenant.Id,
                    tenant.TenantSlug,
                    message = "Tenant created successfully. Activation email sent.",
                    activationUrl = $"{Request.Scheme}://{Request.Host}/auth/activate?slug={tenant.TenantSlug}&token={tenant.ActivationToken}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Activate tenant after admin verifies email.
        /// </summary>
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateAsync([FromBody] ActivateTenantDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenantSlug) ||
                    string.IsNullOrWhiteSpace(request.ActivationToken))
                {
                    return BadRequest("Tenant slug and activation token are required.");
                }

                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var tenant = await _tenantService.ActivateTenantAsync(
                    request.TenantSlug,
                    request.ActivationToken,
                    userId);

                return Ok(new
                {
                    tenant.Id,
                    tenant.Status,
                    message = "Tenant activated successfully. Proceed to organizational profiling."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating tenant");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Save organizational profile (questionnaire answers).
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/org-profile")]
        public async Task<IActionResult> SaveOrgProfileAsync(
            Guid tenantId,
            [FromBody] OrganizationProfileDto request)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var profile = await _onboardingService.SaveOrganizationProfileAsync(
                    tenantId,
                    request.OrganizationType,
                    request.Sector,
                    request.Country ?? "SA",
                    request.DataTypes,
                    request.HostingModel,
                    request.Size,
                    request.Maturity,
                    request.Vendors,
                    new Dictionary<string, string>(),
                    userId);

                return Ok(new
                {
                    profile.Id,
                    profile.TenantId,
                    message = "Organization profile saved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving organization profile");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Complete onboarding and derive scope (invoke rules engine).
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/complete-onboarding")]
        public async Task<IActionResult> CompleteOnboardingAsync(Guid tenantId)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var executionLog = await _onboardingService.CompleteOnboardingAsync(tenantId, userId);

                return Ok(new
                {
                    executionLog.Id,
                    executionLog.Status,
                    message = "Onboarding completed. Scope derived using rules engine."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing onboarding");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Complete smart onboarding - auto-generates assessment templates and GRC plan
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/complete-smart-onboarding")]
        public async Task<IActionResult> CompleteSmartOnboardingAsync(Guid tenantId)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _smartOnboardingService.CompleteSmartOnboardingAsync(tenantId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing smart onboarding");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete FULL smart onboarding - generates assessments, requirements, and team workspaces
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/complete-full-onboarding")]
        public async Task<IActionResult> CompleteFullSmartOnboardingAsync(
            Guid tenantId,
            [FromBody] FullOnboardingRequest? request = null)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _smartOnboardingService.CompleteFullSmartOnboardingAsync(
                    tenantId,
                    userId,
                    request?.TeamMembers);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing full smart onboarding");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get derived scope for tenant (applicable baselines, packages, templates).
        /// Includes ReasonJson explaining why each applies.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/scope")]
        public async Task<IActionResult> GetScopeAsync(Guid tenantId)
        {
            try
            {
                var scope = await _onboardingService.GetDerivedScopeAsync(tenantId);
                return Ok(scope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scope");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Refresh/re-evaluate scope when profile or assets change.
        /// Triggers rules engine to re-derive applicable baselines/packages/templates.
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/refresh-scope")]
        public async Task<IActionResult> RefreshScopeAsync(Guid tenantId, [FromBody] RefreshScopeRequest? request = null)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var reason = request?.Reason ?? "Manual refresh requested";

                var executionLog = await _onboardingService.RefreshScopeAsync(tenantId, userId, reason);

                return Ok(new
                {
                    executionLog.Id,
                    executionLog.Status,
                    Reason = reason,
                    RefreshedAt = DateTime.UtcNow,
                    message = "Scope refreshed successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing scope");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get tenant by slug (used for multi-tenant routing).
        /// </summary>
        [HttpGet("tenants/by-slug/{tenantSlug}")]
        public async Task<IActionResult> GetTenantBySlugAsync(string tenantSlug)
        {
            try
            {
                var tenant = await _tenantService.GetTenantBySlugAsync(tenantSlug);
                if (tenant == null)
                {
                    return NotFound("Tenant not found or not active.");
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get tenant by ID.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}")]
        public async Task<IActionResult> GetTenantAsync(Guid tenantId)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdAsync(tenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant");
                return BadRequest(ex.Message);
            }
        }
    }

    /// <summary>
    /// MVC Controller for onboarding pages (views)
    /// Note: Most actions are public (trial/registration flow), specific actions require auth
    /// </summary>
    [Route("[controller]")]
    public class OnboardingController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            ITenantService tenantService,
            ILogger<OnboardingController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        /// <summary>
        /// MVC Route: Display onboarding index page
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        [AllowAnonymous] // Allow access for trial users before they complete onboarding
        public IActionResult Index()
        {
            return View(nameof(Index));
        }

        /// <summary>
        /// MVC Route: Start onboarding for trial users
        /// Called after trial registration - receives tenantSlug from redirect
        /// </summary>
        [HttpGet("Start/{tenantSlug}")]
        [AllowAnonymous] // Trial users just registered and signed in
        public async Task<IActionResult> Start(string tenantSlug)
        {
            // #region agent log
            try {
                using var logFile = System.IO.File.AppendText("/home/dogan/grc-system/.cursor/debug.log");
                await logFile.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(new {
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "A",
                    location = "OnboardingController.cs:352",
                    message = "Start action called",
                    data = new { tenantSlug, isEmpty = string.IsNullOrWhiteSpace(tenantSlug) },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }));
            } catch {}
            // #endregion

            if (string.IsNullOrWhiteSpace(tenantSlug))
            {
                _logger.LogWarning("Start called with empty tenantSlug");
                return RedirectToAction("Index");
            }

            try
            {
                // Get tenant by slug
                var tenant = await _tenantService.GetTenantBySlugAsync(tenantSlug);

                // #region agent log
                try {
                    using var logFile = System.IO.File.AppendText("/home/dogan/grc-system/.cursor/debug.log");
                    await logFile.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(new {
                        sessionId = "debug-session",
                        runId = "run1",
                        hypothesisId = "B",
                        location = "OnboardingController.cs:365",
                        message = "Tenant lookup result",
                        data = new { tenantSlug, found = tenant != null, tenantId = tenant != null ? tenant.Id.ToString() : null, orgName = tenant?.OrganizationName },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }));
                } catch {}
                // #endregion
                if (tenant == null)
                {
                    _logger.LogWarning("Tenant not found for slug: {TenantSlug}", tenantSlug);
                    TempData["ErrorMessage"] = "Organization not found. Please try again.";
                    return RedirectToAction("Index");
                }

                // Set ViewBag for the view
                ViewBag.TenantSlug = tenantSlug;
                ViewBag.TenantId = tenant.Id;
                ViewBag.OrganizationName = tenant.OrganizationName;
                ViewBag.IsTrial = tenant.IsTrial;
                ViewBag.TrialEndsAt = tenant.TrialEndsAt;
                ViewBag.TrialDaysRemaining = tenant.TrialEndsAt.HasValue 
                    ? (int)(tenant.TrialEndsAt.Value - DateTime.UtcNow).TotalDays 
                    : 0;

                // Store in TempData for subsequent steps
                TempData["TenantId"] = tenant.Id.ToString();
                TempData["TenantSlug"] = tenantSlug;
                TempData["OrganizationName"] = tenant.OrganizationName;

                // #region agent log
                try {
                    using var logFile = System.IO.File.AppendText("/home/dogan/grc-system/.cursor/debug.log");
                    await logFile.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(new {
                        sessionId = "debug-session",
                        runId = "run1",
                        hypothesisId = "C",
                        location = "OnboardingController.cs:374",
                        message = "ViewBag set",
                        data = new {
                            tenantSlug = ViewBag.TenantSlug?.ToString(),
                            tenantId = ViewBag.TenantId?.ToString(),
                            orgName = ViewBag.OrganizationName?.ToString(),
                            isTrial = ViewBag.IsTrial?.ToString(),
                            trialEndsAt = ViewBag.TrialEndsAt?.ToString(),
                            trialDaysRemaining = ViewBag.TrialDaysRemaining?.ToString()
                        },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }));
                } catch {}
                // #endregion

                _logger.LogInformation("Starting onboarding for trial tenant: {TenantSlug} ({TenantId})", 
                    tenantSlug, tenant.Id);

                // Reuse existing Index view which shows the 12-step wizard
                return View("Index");
            }
            catch (Exception ex)
            {
                // #region agent log
                try {
                    using var logFile = System.IO.File.AppendText("/home/dogan/grc-system/.cursor/debug.log");
                    var stackTrace = ex.StackTrace ?? string.Empty;
                    await logFile.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(new {
                        sessionId = "debug-session",
                        runId = "run1",
                        hypothesisId = "B",
                        location = "OnboardingController.cs:403",
                        message = "Start action exception",
                        data = new { tenantSlug, error = ex.Message, stackTrace = stackTrace.Substring(0, Math.Min(500, stackTrace.Length)) },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }));
                } catch {}
                // #endregion
                _logger.LogError(ex, "Error starting onboarding for tenant: {TenantSlug}", tenantSlug);
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// MVC Route: Display signup page
        /// </summary>
        [HttpGet("Signup")]
        [AllowAnonymous] // Must be public for new organization registration
        public IActionResult Signup()
        {
            // #region agent log
            try {
                using var logFile = System.IO.File.AppendText("/home/dogan/grc-system/.cursor/debug.log");
                logFile.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "BUTTON_TEST",
                    location = "OnboardingController.cs:470",
                    message = "BUTTON 3: New Organization clicked - Signup page",
                    data = new { isAuthenticated = User.Identity?.IsAuthenticated },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }));
            } catch {}
            // #endregion

            return View(new CreateTenantDto());
        }

        /// <summary>
        /// MVC Route: Process signup form and redirect to OrgProfile
        /// Creates the tenant in the database immediately.
        /// </summary>
        [HttpPost("Signup")]
        [AllowAnonymous] // Must be public for new organization registration
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(CreateTenantDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Generate tenant slug if not provided
                var tenantSlug = model.TenantSlug ?? model.OrganizationName?.ToLower()
                    .Replace(" ", "-")
                    .Replace("_", "-")
                    .Replace(".", "-")
                    .Trim('-');

                if (string.IsNullOrWhiteSpace(tenantSlug))
                {
                    ModelState.AddModelError(nameof(model.TenantSlug), "Tenant slug is required.");
                    return View(model);
                }

                // CRITICAL: Actually create the tenant in the database
                var tenant = await _tenantService.CreateTenantAsync(
                    model.OrganizationName ?? string.Empty,
                    model.AdminEmail ?? string.Empty,
                    tenantSlug);

                _logger.LogInformation("Tenant created via MVC signup: {TenantId} ({Slug})", tenant.Id, tenant.TenantSlug);

                // Store tenant info in TempData for next step
                TempData["TenantSlug"] = tenant.TenantSlug;
                TempData["OrganizationName"] = tenant.OrganizationName;
                TempData["AdminEmail"] = tenant.AdminEmail;
                TempData["TenantId"] = tenant.Id.ToString();

                return RedirectToAction(nameof(OrgProfile));
            }
            catch (InvalidOperationException ex)
            {
                // Handle duplicate slug or other validation errors
                ModelState.AddModelError("", ex.Message);
                _logger.LogWarning(ex, "Failed to create tenant: {Message}", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                ModelState.AddModelError("", "An error occurred while creating your account. Please try again.");
                _logger.LogError(ex, "Error creating tenant during signup");
                return View(model);
            }
        }

        /// <summary>
        /// MVC Route: Display organization profile page
        /// </summary>
        [HttpGet("OrgProfile")]
        [HttpGet("org-profile")]
        public IActionResult OrgProfile()
        {
            var tenantIdStr = TempData["TenantId"]?.ToString();
            var tenantId = string.IsNullOrEmpty(tenantIdStr) ? Guid.Empty : Guid.Parse(tenantIdStr);
            TempData.Keep("TenantId");
            TempData.Keep("TenantSlug");
            TempData.Keep("OrganizationName");

            return View(new OrganizationProfileDto { TenantId = tenantId });
        }

        /// <summary>
        /// MVC Route: Process organization profile and redirect to ReviewScope
        /// </summary>
        [HttpPost("OrgProfile")]
        [HttpPost("SaveOrgProfile")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveOrgProfile(OrganizationProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(OrgProfile), model);
            }

            // Store profile in TempData for scope derivation
            TempData["OrganizationType"] = model.OrganizationType;
            TempData["Sector"] = model.Sector;
            TempData["Country"] = model.Country;
            TempData["HostingModel"] = model.HostingModel;
            TempData["Size"] = model.Size;
            TempData["TenantId"] = model.TenantId.ToString();
            TempData.Keep("TenantSlug");
            TempData.Keep("OrganizationName");

            return RedirectToAction(nameof(ReviewScope));
        }

        /// <summary>
        /// MVC Route: Display scope review page
        /// </summary>
        [HttpGet("ReviewScope")]
        [HttpGet("review-scope")]
        public IActionResult ReviewScope()
        {
            var tenantIdStr = TempData["TenantId"]?.ToString();
            TempData.Keep("TenantId");
            TempData.Keep("TenantSlug");
            TempData.Keep("OrganizationType");
            TempData.Keep("Sector");

            // Build scope based on profile (simplified - real implementation uses RulesEngine)
            var scope = new OnboardingScopeDto
            {
                ApplicableBaselines = new List<BaselineDto>
                {
                    new BaselineDto { BaselineCode = "NCA-ECC", ReasonJson = "Required for all Saudi organizations" },
                    new BaselineDto { BaselineCode = "PDPL", ReasonJson = "Personal data protection requirement" }
                },
                ApplicablePackages = new List<PackageDto>(),
                ApplicableTemplates = new List<TemplateDto>()
            };

            return View(scope);
        }

        /// <summary>
        /// MVC Route: Process scope acceptance and redirect to CreatePlan
        /// </summary>
        [HttpPost("CompleteOnboarding")]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteOnboarding()
        {
            TempData.Keep("TenantId");
            TempData.Keep("TenantSlug");
            TempData.Keep("OrganizationName");

            return RedirectToAction(nameof(CreatePlan));
        }

        /// <summary>
        /// MVC Route: Display plan creation page
        /// </summary>
        [HttpGet("CreatePlan")]
        [HttpGet("create-plan")]
        public IActionResult CreatePlan()
        {
            var tenantIdStr = TempData["TenantId"]?.ToString();
            var tenantId = string.IsNullOrEmpty(tenantIdStr) ? Guid.Empty : Guid.Parse(tenantIdStr);
            TempData.Keep("TenantId");
            TempData.Keep("TenantSlug");

            return View(new CreatePlanDto { TenantId = tenantId });
        }

        /// <summary>
        /// MVC Route: Process plan creation and redirect to dashboard
        /// </summary>
        [HttpPost("CreatePlan")]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePlan(CreatePlanDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Plan created - redirect to dashboard
            TempData["SuccessMessage"] = "Onboarding complete! Your first assessment plan has been created.";
            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// MVC Route: Display activation page
        /// </summary>
        [HttpGet("Activate")]
        public IActionResult Activate()
        {
            return View();
        }

        /// <summary>
        /// MVC Route: Process activation and redirect to OrgProfile
        /// </summary>
        [HttpPost("Activate")]
        [ValidateAntiForgeryToken]
        public IActionResult Activate(string tenantSlug, string activationToken)
        {
            if (string.IsNullOrEmpty(tenantSlug) || string.IsNullOrEmpty(activationToken))
            {
                ModelState.AddModelError("", "Invalid activation link.");
                return View();
            }

            TempData["TenantSlug"] = tenantSlug;
            TempData["Activated"] = true;

            return RedirectToAction(nameof(OrgProfile));
        }
    }

    /// <summary>
    /// Request for full onboarding with team members
    /// </summary>
    public class FullOnboardingRequest
    {
        public List<TeamMemberDto>? TeamMembers { get; set; }
    }

    /// <summary>
    /// Request for scope refresh/re-evaluation
    /// </summary>
    public class RefreshScopeRequest
    {
        /// <summary>
        /// Reason for the scope refresh (e.g., "Profile updated", "New assets added", "Manual refresh")
        /// </summary>
        public string? Reason { get; set; }
    }
}
