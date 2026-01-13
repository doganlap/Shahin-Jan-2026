using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service interface for tenant self-registration.
    /// Follows ABP pattern from support tickets #7077 and #8116.
    /// </summary>
    public interface ITenantRegistrationService
    {
        /// <summary>
        /// Creates a new tenant with admin user atomically.
        /// Returns both tenant and user for auto-sign in.
        /// </summary>
        Task<(Tenant tenant, ApplicationUser adminUser)> CreateTenantWithAdminAsync(
            string companyName,
            string tenantSlug,
            string adminEmail,
            string adminPassword,
            string adminFirstName,
            string adminLastName,
            string edition = "Trial");

        /// <summary>
        /// Validates tenant slug format (lowercase, alphanumeric + hyphens).
        /// </summary>
        bool IsValidTenantSlug(string slug);

        /// <summary>
        /// Generates tenant slug from company name.
        /// Example: "Acme Corporation" → "acme-corporation"
        /// </summary>
        string GenerateTenantSlug(string companyName);
    }
}
