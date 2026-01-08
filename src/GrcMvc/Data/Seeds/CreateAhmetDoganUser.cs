using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Helper to create Ahmet Dogan user (Platform Admin)
/// Can be called from ApplicationInitializer or SeedController
/// </summary>
public static class CreateAhmetDoganUser
{
    public static async Task<bool> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        GrcDbContext context,
        ILogger logger)
    {
        try
        {
            const string email = "ahmet.dogan@doganconsult.com";
            const string password = "DogCon@Admin2026";

            // Use CreateUserHelper which handles all the tenant linking and role assignment
            var user = await CreateUserHelper.CreateUserAsync(
                userManager,
                context,
                logger,
                firstName: "Ahmet",
                lastName: "Dogan",
                email: email,
                password: password,
                department: "Administration",
                jobTitle: "Platform Administrator",
                roleName: "PlatformAdmin",
                tenantId: null // Will use default tenant
            );

            if (user != null)
            {
                logger.LogInformation($"✅ Ahmet Dogan user created successfully: {email}");
                return true;
            }
            else
            {
                logger.LogWarning($"⚠️ Failed to create Ahmet Dogan user: {email}");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error creating Ahmet Dogan user");
            return false;
        }
    }
}
