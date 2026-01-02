using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Grc.Domain.Seed;

public class GrcAdminUserDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IdentityUserManager _userManager;
    private readonly IConfiguration _configuration;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<GrcAdminUserDataSeedContributor> _logger;

    public GrcAdminUserDataSeedContributor(
        IdentityUserManager userManager,
        IConfiguration configuration,
        IGuidGenerator guidGenerator,
        ILogger<GrcAdminUserDataSeedContributor> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Get admin credentials from configuration or use defaults
        var adminUserName = _configuration["AdminUser:UserName"] ?? "admin";
        var adminEmail = _configuration["AdminUser:Email"] ?? "admin@grc.local";
        var adminPassword = _configuration["AdminUser:Password"] ?? "1q2w3E*"; // Default password - CHANGE IN PRODUCTION!
        var adminName = _configuration["AdminUser:Name"] ?? "System";
        var adminSurname = _configuration["AdminUser:Surname"] ?? "Administrator";

        // Check if admin user already exists
        var existingUser = await _userManager.FindByNameAsync(adminUserName);
        if (existingUser != null)
        {
            _logger.LogInformation("Admin user '{UserName}' already exists, skipping creation", adminUserName);
            return;
        }

        _logger.LogInformation("Creating admin user '{UserName}'", adminUserName);

        // Create admin user
        var adminUser = new IdentityUser(
            _guidGenerator.Create(),
            adminUserName,
            adminEmail,
            context.TenantId
        )
        {
            Name = adminName,
            Surname = adminSurname
        };

        // Set email confirmed and active using ABP 8.x compatible methods
        adminUser.SetEmailConfirmed(true);
        adminUser.SetIsActive(true);

        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create admin user: {Errors}", errors);
            throw new Exception($"Failed to create admin user: {errors}");
        }

        _logger.LogInformation("Admin user created successfully");

        // Assign SuperAdmin role to admin user
        var userRoles = await _userManager.GetRolesAsync(adminUser);
        if (!userRoles.Contains("SuperAdmin"))
        {
            var roleResult = await _userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            if (roleResult.Succeeded)
            {
                _logger.LogInformation("Assigned SuperAdmin role to admin user");
            }
            else
            {
                _logger.LogWarning("SuperAdmin role not found or failed to assign");
            }
        }
    }
}
