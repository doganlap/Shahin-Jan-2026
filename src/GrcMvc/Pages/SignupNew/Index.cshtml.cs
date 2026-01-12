using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Volo.Abp.TenantManagement;
using Volo.Abp.MultiTenancy;
using GrcMvc.Models.Entities;
using GrcMvc.Data;

namespace GrcMvc.Pages.SignupNew
{
    /// <summary>
    /// Trial registration using ABP Framework's built-in tenant management
    /// Route: /SignupNew
    /// </summary>
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ITenantManager _tenantManager;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentTenant _currentTenant;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly GrcDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITenantManager tenantManager,
            ITenantRepository tenantRepository,
            ICurrentTenant currentTenant,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            GrcDbContext context,
            ILogger<IndexModel> logger)
        {
            _tenantManager = tenantManager;
            _tenantRepository = tenantRepository;
            _currentTenant = currentTenant;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public TrialInputModel Input { get; set; } = new TrialInputModel();

        public string? ErrorMessage { get; set; }
        public bool ShowError => !string.IsNullOrEmpty(ErrorMessage);

        public void OnGet()
        {
            // Display the registration form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 1. Sanitize company name to create base slug
                var baseSlug = SanitizeCompanyName(Input.CompanyName);
                if (string.IsNullOrEmpty(baseSlug))
                {
                    ModelState.AddModelError(string.Empty, "Company name must contain at least one alphanumeric character.");
                    return Page();
                }

                // 2. Generate unique slug (with suffix if needed)
                var uniqueSlug = await GenerateUniqueSlugAsync(baseSlug);

                // 3. Create ABP Tenant using ITenantManager
                var tenantId = Guid.NewGuid();
                var abpTenant = await _tenantManager.CreateAsync(uniqueSlug, tenantId);
                await _tenantRepository.InsertAsync(abpTenant);

                _logger.LogInformation("ABP Tenant created: {TenantId} - {TenantName}", tenantId, uniqueSlug);

                // 4. Create custom Tenant record (for GRC-specific fields like trial info)
                var customTenant = new Tenant
                {
                    Id = tenantId, // Same ID as ABP tenant
                    TenantSlug = uniqueSlug,
                    OrganizationName = Input.CompanyName,
                    AdminEmail = Input.Email,
                    Status = "Active",
                    SubscriptionTier = "Trial",
                    IsTrial = true,
                    TrialStartsAt = DateTime.UtcNow,
                    TrialEndsAt = DateTime.UtcNow.AddDays(7),
                    BillingStatus = "Trialing",
                    IsActive = true,
                    ActivatedAt = DateTime.UtcNow,
                    SubscriptionStartDate = DateTime.UtcNow,
                    SubscriptionEndDate = DateTime.UtcNow.AddDays(7),
                    OnboardingStatus = "NOT_STARTED",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "SignupNew"
                };
                _context.Tenants.Add(customTenant);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Custom Tenant created: {TenantId}", tenantId);

                // 5. Create admin user within tenant context
                ApplicationUser adminUser;
                using (_currentTenant.Change(tenantId))
                {
                    var nameParts = (Input.FullName ?? Input.Email).Split(' ', 2);
                    adminUser = new ApplicationUser
                    {
                        UserName = Input.Email,
                        Email = Input.Email,
                        EmailConfirmed = !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction(),
                        FirstName = nameParts.Length > 0 ? nameParts[0] : Input.Email,
                        LastName = nameParts.Length > 1 ? nameParts[1] : "",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, Input.Password);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to create admin user: {Errors}", errors);
                        ModelState.AddModelError(string.Empty, $"Registration failed: {errors}");

                        // Rollback: Delete tenant if user creation fails
                        await _tenantRepository.DeleteAsync(abpTenant);
                        _context.Tenants.Remove(customTenant);
                        await _context.SaveChangesAsync();

                        return Page();
                    }

                    // 6. Assign TenantAdmin role
                    if (!await _roleManager.RoleExistsAsync("TenantAdmin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("TenantAdmin"));
                    }
                    await _userManager.AddToRoleAsync(adminUser, "TenantAdmin");

                    // Reload user to get persisted ID
                    adminUser = await _userManager.FindByEmailAsync(Input.Email);
                    if (adminUser == null)
                    {
                        throw new InvalidOperationException("User was created but could not be retrieved.");
                    }

                    _logger.LogInformation("Admin user created: {UserId} - {Email}", adminUser.Id, Input.Email);
                }

                // 7. Create TenantUser linkage
                var tenantUser = new TenantUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = adminUser.Id,
                    RoleCode = GrcMvc.Constants.RoleConstants.TenantAdmin,
                    TitleCode = "ADMIN",
                    Status = "Active",
                    ActivatedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantUsers.Add(tenantUser);
                await _context.SaveChangesAsync();

                // 8. Auto-login with TenantId claim
                await _signInManager.SignInAsync(adminUser, isPersistent: true);

                // Add TenantId claim for tenant resolution
                var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(adminUser);
                var identity = claimsPrincipal.Identity as System.Security.Claims.ClaimsIdentity;
                if (identity != null && !identity.HasClaim(c => c.Type == "TenantId"))
                {
                    identity.AddClaim(new System.Security.Claims.Claim("TenantId", tenantId.ToString()));
                    await HttpContext.SignInAsync(
                        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                        claimsPrincipal);
                }

                _logger.LogInformation("Trial registered successfully via /SignupNew: TenantId={TenantId}, Email={Email}, Slug={Slug}",
                    tenantId, Input.Email, uniqueSlug);

                // 9. Redirect to onboarding wizard
                return RedirectToAction("Start", "Onboarding", new { tenantSlug = uniqueSlug });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trial registration failed via /SignupNew: {Email}", Input.Email);
                ErrorMessage = "Registration failed. Please try again. If the problem persists, contact support.";
                return Page();
            }
        }

        /// <summary>
        /// Sanitizes company name to create a valid slug (alphanumeric only)
        /// </summary>
        private string SanitizeCompanyName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return string.Empty;

            // Convert to lowercase and remove all non-alphanumeric characters
            var sanitized = Regex.Replace(companyName.ToLowerInvariant(), @"[^a-z0-9]", "");

            // Limit length to 50 characters
            return sanitized.Length > 50 ? sanitized.Substring(0, 50) : sanitized;
        }

        /// <summary>
        /// Generates a unique slug by checking both ABP tenants and custom tenants
        /// If slug exists, appends a numeric suffix (acme → acme2 → acme3)
        /// </summary>
        private async Task<string> GenerateUniqueSlugAsync(string baseSlug)
        {
            var slug = baseSlug;
            var suffix = 2;

            while (await IsSlugTakenAsync(slug))
            {
                slug = $"{baseSlug}{suffix}";
                suffix++;

                // Safety limit to prevent infinite loops
                if (suffix > 1000)
                {
                    throw new InvalidOperationException("Unable to generate unique slug after 1000 attempts.");
                }
            }

            return slug;
        }

        /// <summary>
        /// Checks if a slug is already taken in either ABP tenants or custom tenants
        /// </summary>
        private async Task<bool> IsSlugTakenAsync(string slug)
        {
            // Check ABP tenants
            var abpTenantExists = await _tenantRepository.FindByNameAsync(slug) != null;
            if (abpTenantExists)
                return true;

            // Check custom tenants
            var customTenantExists = await _context.Tenants.AnyAsync(t => t.TenantSlug == slug);
            return customTenantExists;
        }
    }

    /// <summary>
    /// Input model for trial registration form
    /// </summary>
    public class TrialInputModel
    {
        [Required(ErrorMessage = "Company name is required")]
        [Display(Name = "Company Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Work Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "I accept the Terms and Conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }
    }
}
