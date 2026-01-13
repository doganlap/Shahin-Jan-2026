using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for tenant self-registration following ABP pattern.
    /// Implements the flow from ABP support tickets #7077 and #8116.
    /// Creates tenant + admin user atomically with TenantUser association.
    /// </summary>
    public class TenantRegistrationService : ITenantRegistrationService
    {
        private readonly GrcDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TenantRegistrationService> _logger;
        private readonly IEmailService _emailService;
        private readonly IAuditEventService _auditService;

        public TenantRegistrationService(
            GrcDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TenantRegistrationService> logger,
            IEmailService emailService,
            IAuditEventService auditService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
            _auditService = auditService;
        }

        /// <summary>
        /// Creates a new tenant with admin user following ABP pattern.
        /// This is an atomic operation - all or nothing.
        /// </summary>
        /// <param name="companyName">Company/organization name</param>
        /// <param name="tenantSlug">Unique tenant identifier (lowercase, alphanumeric + hyphens)</param>
        /// <param name="adminEmail">Admin user email</param>
        /// <param name="adminPassword">Admin user password</param>
        /// <param name="adminFirstName">Admin first name</param>
        /// <param name="adminLastName">Admin last name</param>
        /// <param name="edition">Tenant edition (Trial, Free, Professional, Enterprise). Default: Trial</param>
        /// <returns>Tuple of created tenant and admin user</returns>
        public async Task<(Tenant tenant, ApplicationUser adminUser)> CreateTenantWithAdminAsync(
            string companyName,
            string tenantSlug,
            string adminEmail,
            string adminPassword,
            string adminFirstName,
            string adminLastName,
            string edition = "Trial")
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(companyName))
                throw new ArgumentException("Company name is required", nameof(companyName));
            if (string.IsNullOrWhiteSpace(tenantSlug))
                throw new ArgumentException("Tenant slug is required", nameof(tenantSlug));
            if (string.IsNullOrWhiteSpace(adminEmail))
                throw new ArgumentException("Admin email is required", nameof(adminEmail));
            if (string.IsNullOrWhiteSpace(adminPassword))
                throw new ArgumentException("Admin password is required", nameof(adminPassword));

            // Normalize slug
            tenantSlug = tenantSlug.ToLower().Trim();

            // Start transaction - ATOMIC OPERATION
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Starting tenant self-registration for {CompanyName} ({TenantSlug})", companyName, tenantSlug);

                // ═══════════════════════════════════════════════════════════
                // STEP 1: Validate tenant slug is unique
                // ═══════════════════════════════════════════════════════════
                var existingTenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantSlug.ToLower() == tenantSlug);

                if (existingTenant != null)
                {
                    throw new InvalidOperationException($"Tenant with slug '{tenantSlug}' already exists");
                }

                // ═══════════════════════════════════════════════════════════
                // STEP 2: Validate admin email is unique
                // ═══════════════════════════════════════════════════════════
                var existingUser = await _userManager.FindByEmailAsync(adminEmail);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email '{adminEmail}' already exists");
                }

                // ═══════════════════════════════════════════════════════════
                // STEP 3: Create Tenant
                // ═══════════════════════════════════════════════════════════
                var tenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    OrganizationName = companyName,
                    TenantSlug = tenantSlug,
                    AdminEmail = adminEmail,
                    Status = "Active", // Immediate activation (no email confirmation needed)
                    Edition = edition, // Trial, Free, Professional, Enterprise
                    TrialStartedAt = edition == "Trial" ? DateTime.UtcNow : null,
                    TrialEndsAt = edition == "Trial" ? DateTime.UtcNow.AddDays(14) : null,
                    IsTrialExpired = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "SYSTEM_REGISTRATION",
                    IsDeleted = false
                };

                await _context.Tenants.AddAsync(tenant);
                await _context.SaveChangesAsync(); // Save to get tenant.Id for foreign keys

                _logger.LogInformation("Created tenant {TenantId} ({TenantSlug})", tenant.Id, tenant.TenantSlug);

                // ═══════════════════════════════════════════════════════════
                // STEP 4: Create Admin User FOR THIS TENANT
                // ═══════════════════════════════════════════════════════════
                var adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true, // Auto-confirm for self-registration
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    IsActive = true,
                    MustChangePassword = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "SYSTEM_REGISTRATION"
                };

                var createUserResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create admin user: {errors}");
                }

                _logger.LogInformation("Created admin user {UserId} ({Email})", adminUser.Id, adminUser.Email);

                // ═══════════════════════════════════════════════════════════
                // STEP 5: Create TenantUser Association (CRITICAL!)
                // ═══════════════════════════════════════════════════════════
                // This is what links the user to the tenant
                var tenantUser = new TenantUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    UserId = Guid.Parse(adminUser.Id),
                    Role = "Owner", // First user is Owner
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "SYSTEM_REGISTRATION"
                };

                await _context.TenantUsers.AddAsync(tenantUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created TenantUser association for {UserId} → {TenantId}", adminUser.Id, tenant.Id);

                // ═══════════════════════════════════════════════════════════
                // STEP 6: Assign Owner Role to Admin User
                // ═══════════════════════════════════════════════════════════
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Owner");
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign Owner role to user {UserId}. Continuing anyway.", adminUser.Id);
                }

                _logger.LogInformation("Assigned Owner role to user {UserId}", adminUser.Id);

                // ═══════════════════════════════════════════════════════════
                // STEP 7: Create Default Workspace for Tenant
                // ═══════════════════════════════════════════════════════════
                var workspace = new Workspace
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Name = $"{companyName} Workspace",
                    Description = "Default workspace for organization",
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsDeleted = false
                };

                await _context.Workspaces.AddAsync(workspace);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created default workspace {WorkspaceId} for tenant {TenantId}", workspace.Id, tenant.Id);

                // ═══════════════════════════════════════════════════════════
                // STEP 8: Seed Default Tenant Data
                // ═══════════════════════════════════════════════════════════
                await SeedTenantDefaultDataAsync(tenant.Id, workspace.Id, adminUser.Id);

                // ═══════════════════════════════════════════════════════════
                // STEP 9: Commit Transaction (ALL OR NOTHING)
                // ═══════════════════════════════════════════════════════════
                await transaction.CommitAsync();

                _logger.LogInformation("✅ Successfully completed tenant self-registration for {TenantId}", tenant.Id);

                // ═══════════════════════════════════════════════════════════
                // STEP 10: Send Welcome Email (async, don't block registration)
                // ═══════════════════════════════════════════════════════════
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendWelcomeEmailAsync(tenant, adminUser);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send welcome email to {Email}", adminEmail);
                    }
                });

                // ═══════════════════════════════════════════════════════════
                // STEP 11: Audit Event
                // ═══════════════════════════════════════════════════════════
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _auditService.LogAsync(
                            "TenantRegistration",
                            "TenantCreated",
                            tenant.Id.ToString(),
                            $"New tenant '{companyName}' registered via self-registration",
                            "SYSTEM_REGISTRATION",
                            tenant.Id
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log audit event for tenant registration");
                    }
                });

                return (tenant, adminUser);
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();

                _logger.LogError(ex, "❌ Tenant self-registration failed for {CompanyName} ({TenantSlug})", companyName, tenantSlug);
                throw;
            }
        }

        /// <summary>
        /// Seeds default data for a newly created tenant.
        /// Creates default risk categories, compliance frameworks, etc.
        /// </summary>
        private async Task SeedTenantDefaultDataAsync(Guid tenantId, Guid workspaceId, string createdBy)
        {
            _logger.LogInformation("Seeding default data for tenant {TenantId}", tenantId);

            try
            {
                // TODO: Add default risk categories
                // TODO: Add default compliance frameworks (NIST, ISO 27001, etc.)
                // TODO: Add default control types
                // TODO: Add default assessment templates

                // For now, just log - actual seeding can be added later
                _logger.LogInformation("Default data seeding skipped (not yet implemented)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed default data for tenant {TenantId}", tenantId);
                // Don't throw - seeding failure should not block tenant creation
            }
        }

        /// <summary>
        /// Sends welcome email to new tenant admin.
        /// </summary>
        private async Task SendWelcomeEmailAsync(Tenant tenant, ApplicationUser adminUser)
        {
            try
            {
                var subject = $"Welcome to Shahin AI GRC - {tenant.OrganizationName}";
                var body = $@"
                    <h2>Welcome to Shahin AI GRC Platform!</h2>
                    <p>Hello {adminUser.FirstName},</p>
                    <p>Your organization <strong>{tenant.OrganizationName}</strong> has been successfully registered.</p>
                    <p><strong>Your Account Details:</strong></p>
                    <ul>
                        <li>Organization: {tenant.OrganizationName}</li>
                        <li>Tenant ID: {tenant.TenantSlug}</li>
                        <li>Edition: {tenant.Edition}</li>
                        <li>Email: {adminUser.Email}</li>
                        <li>Role: Owner</li>
                    </ul>
                    {(tenant.Edition == "Trial" ? $"<p><strong>Trial Period:</strong> Your 14-day trial expires on {tenant.TrialEndsAt:MMMM dd, yyyy}.</p>" : "")}
                    <p><a href=\"/Onboarding\">Click here to complete your onboarding</a></p>
                    <p>Best regards,<br/>Shahin AI GRC Team</p>
                ";

                await _emailService.SendEmailAsync(adminUser.Email, subject, body);
                _logger.LogInformation("Welcome email sent to {Email}", adminUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", adminUser.Email);
                // Don't throw - email failure should not block registration
            }
        }

        /// <summary>
        /// Validates tenant slug format (lowercase, alphanumeric + hyphens).
        /// </summary>
        public bool IsValidTenantSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            slug = slug.Trim().ToLower();

            // Must be 3-50 characters, alphanumeric + hyphens, no spaces
            if (slug.Length < 3 || slug.Length > 50)
                return false;

            // Must start and end with alphanumeric
            if (!char.IsLetterOrDigit(slug[0]) || !char.IsLetterOrDigit(slug[^1]))
                return false;

            // Only allow alphanumeric and hyphens
            return slug.All(c => char.IsLetterOrDigit(c) || c == '-');
        }

        /// <summary>
        /// Generates tenant slug from company name.
        /// Example: "Acme Corporation" → "acme-corporation"
        /// </summary>
        public string GenerateTenantSlug(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return string.Empty;

            // Convert to lowercase, replace spaces with hyphens
            var slug = companyName.ToLower()
                .Replace(" ", "-")
                .Replace("_", "-");

            // Remove non-alphanumeric characters (except hyphens)
            slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

            // Remove multiple consecutive hyphens
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            // Ensure max 50 characters
            if (slug.Length > 50)
                slug = slug.Substring(0, 50).TrimEnd('-');

            return slug;
        }
    }
}
