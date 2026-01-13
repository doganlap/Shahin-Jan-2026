# How ABP Creates Tenants in Multi-Tenant Systems

**Date:** 2026-01-13
**Question:** How does ABP handle creating tenants during registration?
**Answer:** ABP uses the TenantManager pattern with automatic provisioning

---

## 🏗️ ABP's Multi-Tenant Registration Flow

### Standard ABP Approach (with Volo.Saas Module)

```
User visits registration page
  ↓
Fills form:
  - Tenant Name (Company Name)
  - Admin Email
  - Admin Password
  - Admin Full Name
  ↓
System calls ITenantManager.CreateAsync()
  ↓
TenantManager does:
  1. Creates Tenant record
  2. Creates Admin User for that tenant
  3. Sets ConnectionString (if database-per-tenant)
  4. Creates Edition/Subscription
  5. Seeds default data for tenant
  6. Creates default roles
  7. Assigns admin to Owner role
  ↓
Tenant is fully provisioned and ready
```

---

## 📋 ABP's Database Structure

### 1. Tenant Table (Host Database)

```sql
CREATE TABLE AbpTenants (
    Id UUID PRIMARY KEY,
    Name VARCHAR(64) NOT NULL,          -- Tenant name (company name)
    NormalizedName VARCHAR(64),          -- For searching
    IsActive BOOLEAN DEFAULT TRUE,
    EditionId UUID NULL,                 -- Links to Edition (Free/Pro/Enterprise)

    -- Connection String (if separate database per tenant)
    ConnectionStrings JSONB NULL,        -- {"Default": "Host=...;Database=Tenant_ABC"}

    -- Timestamps
    CreationTime TIMESTAMP NOT NULL,
    CreatorId UUID NULL,
    LastModificationTime TIMESTAMP NULL,

    -- Soft delete
    IsDeleted BOOLEAN DEFAULT FALSE,
    DeletionTime TIMESTAMP NULL
);
```

### 2. TenantConnectionString Table

```sql
CREATE TABLE AbpTenantConnectionStrings (
    TenantId UUID NOT NULL REFERENCES AbpTenants(Id),
    Name VARCHAR(64) NOT NULL,           -- "Default", "Logging", etc.
    Value VARCHAR(1024) NOT NULL,        -- Connection string
    PRIMARY KEY (TenantId, Name)
);
```

### 3. User Table (Per-Tenant or Shared)

```sql
CREATE TABLE AbpUsers (
    Id UUID PRIMARY KEY,
    TenantId UUID NULL,                  -- NULL = host user, Value = tenant user
    UserName VARCHAR(256) NOT NULL,
    Email VARCHAR(256) NOT NULL,
    EmailConfirmed BOOLEAN DEFAULT FALSE,
    PasswordHash VARCHAR(256),

    -- User info
    Name VARCHAR(64),
    Surname VARCHAR(64),
    PhoneNumber VARCHAR(16),

    -- Timestamps
    CreationTime TIMESTAMP NOT NULL,

    -- Soft delete
    IsDeleted BOOLEAN DEFAULT FALSE
);
```

---

## 💻 ABP Code Examples

### 1. ABP's TenantManager (Simplified)

```csharp
// ABP Framework: Volo.Abp.TenantManagement.TenantManager
public class TenantManager : ITenantManager
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    public async Task<Tenant> CreateAsync(string name, Guid? editionId = null)
    {
        // 1. Validate tenant name is unique
        if (await _tenantRepository.FindByNameAsync(name) != null)
        {
            throw new BusinessException("TenantNameAlreadyExists");
        }

        // 2. Create tenant
        var tenant = new Tenant(
            GuidGenerator.Create(),
            name
        );

        // 3. Set edition (Free/Professional/Enterprise)
        if (editionId.HasValue)
        {
            tenant.SetEditionId(editionId.Value);
        }

        // 4. Save tenant
        await _tenantRepository.InsertAsync(tenant);

        return tenant;
    }

    public async Task<Tenant> CreateAsync(
        string name,
        string adminEmail,
        string adminPassword,
        Guid? editionId = null)
    {
        // 1. Create tenant
        var tenant = await CreateAsync(name, editionId);

        // 2. Switch to tenant context (important!)
        using (_currentTenant.Change(tenant.Id))
        {
            // 3. Create admin user FOR THIS TENANT
            var adminUser = new IdentityUser(
                GuidGenerator.Create(),
                adminEmail,
                adminEmail,
                tenant.Id  // CRITICAL: Set TenantId
            );

            await _userManager.CreateAsync(adminUser, adminPassword);

            // 4. Assign admin role
            await _userManager.AddToRoleAsync(adminUser, "admin");
        }

        return tenant;
    }
}
```

---

### 2. ABP's Registration Controller

```csharp
// ABP SaaS Module: Volo.Saas.Host.RegisterController
[AllowAnonymous]
public class RegisterController : AbpController
{
    private readonly ITenantManager _tenantManager;
    private readonly IIdentityUserAppService _userAppService;

    [HttpPost]
    public async Task<IActionResult> Register(RegisterTenantDto input)
    {
        // 1. Validate input
        if (await _tenantRepository.FindByNameAsync(input.TenantName) != null)
        {
            throw new UserFriendlyException("Tenant name already exists");
        }

        // 2. Create tenant with admin user
        var tenant = await _tenantManager.CreateAsync(
            name: input.TenantName,
            adminEmail: input.AdminEmail,
            adminPassword: input.AdminPassword,
            editionId: input.EditionId
        );

        // 3. Seed default data for tenant
        await _tenantDataSeeder.SeedAsync(tenant.Id);

        // 4. Send welcome email
        await _emailSender.SendAsync(
            to: input.AdminEmail,
            subject: "Welcome to the platform",
            body: $"Your tenant '{tenant.Name}' has been created!"
        );

        // 5. Auto-login the admin user
        await _signInManager.SignInAsync(
            await _userManager.FindByEmailAsync(input.AdminEmail),
            isPersistent: false
        );

        return RedirectToAction("Index", "Dashboard");
    }
}
```

---

### 3. RegisterTenantDto (Input Model)

```csharp
public class RegisterTenantDto
{
    [Required]
    [StringLength(64)]
    public string TenantName { get; set; }  // Company name

    [Required]
    [EmailAddress]
    public string AdminEmail { get; set; }

    [Required]
    [StringLength(128)]
    public string AdminPassword { get; set; }

    [Required]
    public string AdminFullName { get; set; }

    public Guid? EditionId { get; set; }  // Optional: Free/Pro/Enterprise

    // Optional fields
    public string PhoneNumber { get; set; }
    public string Country { get; set; }
}
```

---

## 🔑 Key Concepts in ABP's Approach

### 1. ICurrentTenant Service

**Critical for multi-tenant operations:**

```csharp
public interface ICurrentTenant
{
    bool IsAvailable { get; }
    Guid? Id { get; }
    string Name { get; }

    // Switch tenant context
    IDisposable Change(Guid? tenantId, string name = null);
}

// Usage:
using (_currentTenant.Change(tenantId))
{
    // All operations here run in tenant context
    var user = await _userRepository.GetAsync(userId);  // Gets user from tenant
}
```

**Why important:**
- All queries automatically filtered by TenantId
- Data isolation guaranteed
- No manual TenantId checking needed

---

### 2. Tenant Isolation Strategies

ABP supports 3 strategies:

#### Strategy A: Shared Database (Most Common)

```
All tenants in same database, filtered by TenantId column

Database: AppDb
  Tables:
    - Tenants (Id, Name)
    - Users (Id, TenantId, Email)
    - Orders (Id, TenantId, ...)
    - Products (Id, TenantId, ...)

Query: SELECT * FROM Users WHERE TenantId = 'ABC-123'
```

**Pros:** Simple, cost-effective
**Cons:** All tenants share resources

---

#### Strategy B: Database-Per-Tenant

```
Each tenant gets separate database

Databases:
  - Host_DB (stores tenant list + connection strings)
  - Tenant_ABC_DB (tenant ABC's data)
  - Tenant_XYZ_DB (tenant XYZ's data)

AbpTenants table:
  Id: ABC-123
  Name: "Acme Corp"
  ConnectionString: "Host=...;Database=Tenant_ABC_DB"
```

**Pros:** Complete isolation, better performance per tenant
**Cons:** More expensive, complex backups

---

#### Strategy C: Hybrid (Schema-Per-Tenant)

```
Same database, different schema per tenant

Database: AppDb
  Schemas:
    - dbo (host data)
    - tenant_abc (tenant ABC's data)
    - tenant_xyz (tenant XYZ's data)

Query: SELECT * FROM tenant_abc.Users
```

**Pros:** Good isolation, manageable cost
**Cons:** Complex migrations

---

### 3. TenantDataSeeder Pattern

```csharp
public class TenantDataSeeder
{
    public async Task SeedAsync(Guid tenantId)
    {
        // Switch to tenant context
        using (_currentTenant.Change(tenantId))
        {
            // Seed roles
            await SeedRolesAsync();

            // Seed permissions
            await SeedPermissionsAsync();

            // Seed default settings
            await SeedSettingsAsync();

            // Seed sample data (if needed)
            if (_configuration["SeedSampleData"] == "true")
            {
                await SeedSampleDataAsync();
            }
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "admin", "manager", "user" };
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private async Task SeedPermissionsAsync()
    {
        // Assign default permissions to roles
        var adminRole = await _roleManager.FindByNameAsync("admin");
        var permissions = _permissionDefinitionManager.GetPermissions();

        foreach (var permission in permissions)
        {
            await _permissionManager.SetForRoleAsync(
                adminRole.Name,
                permission.Name,
                true  // Grant
            );
        }
    }
}
```

---

## 🎯 What You Should Do (ABP Best Practices)

### Step 1: Create Tenant + Admin User Service

```csharp
// Create: Services/TenantCreationService.cs
public class TenantCreationService
{
    private readonly GrcDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TenantCreationService> _logger;

    public async Task<(Tenant tenant, ApplicationUser adminUser)> CreateTenantWithAdminAsync(
        string tenantName,
        string adminEmail,
        string adminPassword,
        string adminFirstName,
        string adminLastName,
        string edition = "Trial")
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Create Tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = tenantName,
                Edition = edition,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,

                // Trial settings
                TrialStartedAt = DateTime.UtcNow,
                TrialEndsAt = DateTime.UtcNow.AddDays(14),
                IsTrialExpired = false,

                // Features (Professional features for trial)
                Features = GetEditionFeatures(edition)
            };

            await _context.Tenants.AddAsync(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created tenant {TenantId} with name {TenantName}",
                tenant.Id, tenant.Name);

            // 2. Create Admin User FOR THIS TENANT
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = adminFirstName,
                LastName = adminLastName,
                EmailConfirmed = true,  // Auto-confirm for trial
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            _logger.LogInformation("Created admin user {UserId} for tenant {TenantId}",
                adminUser.Id, tenant.Id);

            // 3. Create TenantUser Association (CRITICAL!)
            var tenantUser = new TenantUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                UserId = adminUser.Id,
                Role = "Owner",
                Status = "Active",
                JoinedAt = DateTime.UtcNow
            };

            await _context.TenantUsers.AddAsync(tenantUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Associated user {UserId} with tenant {TenantId} as Owner",
                adminUser.Id, tenant.Id);

            // 4. Assign Owner Role
            await _userManager.AddToRoleAsync(adminUser, "Owner");

            // 5. Create Default Workspace
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Main Workspace",
                Description = "Default workspace for your organization",
                IsDefault = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created default workspace {WorkspaceId} for tenant {TenantId}",
                workspace.Id, tenant.Id);

            // 6. Seed Default Data (optional)
            await SeedTenantDefaultDataAsync(tenant.Id);

            // 7. Commit transaction
            await transaction.CommitAsync();

            _logger.LogInformation("Successfully provisioned tenant {TenantId}", tenant.Id);

            return (tenant, adminUser);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create tenant and admin user");
            throw;
        }
    }

    private async Task SeedTenantDefaultDataAsync(Guid tenantId)
    {
        // Seed default roles, permissions, settings for this tenant
        // This runs in the context of the new tenant

        // Example: Create default compliance frameworks
        var frameworks = new List<string> { "ISO 27001", "NIST", "GDPR" };
        foreach (var frameworkName in frameworks)
        {
            var framework = new Framework
            {
                TenantId = tenantId,
                Name = frameworkName,
                IsActive = true,
                // ... other properties
            };
            await _context.Frameworks.AddAsync(framework);
        }

        await _context.SaveChangesAsync();
    }

    private string GetEditionFeatures(string edition)
    {
        return edition switch
        {
            "Trial" => JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["Grc.WorkspaceLimit"] = "5",
                ["Grc.UserLimit"] = "50",
                ["Grc.AIAgentQueryLimit"] = "500",
                ["Grc.AdvancedReporting"] = "true",
                ["Grc.RiskAnalytics"] = "true"
            }),
            "Professional" => JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["Grc.WorkspaceLimit"] = "5",
                ["Grc.UserLimit"] = "50",
                ["Grc.AIAgentQueryLimit"] = "500",
                ["Grc.AdvancedReporting"] = "true",
                ["Grc.RiskAnalytics"] = "true"
            }),
            _ => JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["Grc.WorkspaceLimit"] = "1",
                ["Grc.UserLimit"] = "5",
                ["Grc.AIAgentQueryLimit"] = "10",
                ["Grc.AdvancedReporting"] = "false",
                ["Grc.RiskAnalytics"] = "false"
            })
        };
    }
}
```

---

### Step 2: Update Register Controller

```csharp
[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    // Use the TenantCreationService
    var tenantCreationService = HttpContext.RequestServices
        .GetRequiredService<TenantCreationService>();

    try
    {
        // Create tenant + admin user
        var (tenant, adminUser) = await tenantCreationService.CreateTenantWithAdminAsync(
            tenantName: model.CompanyName,
            adminEmail: model.Email,
            adminPassword: model.Password,
            adminFirstName: model.FirstName,
            adminLastName: model.LastName,
            edition: "Trial"  // 14-day trial
        );

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(
            model.Email,
            $"{model.FirstName} {model.LastName}",
            tenant.Name,
            tenant.TrialEndsAt
        );

        // Auto-login
        await _signInManager.SignInAsync(adminUser, isPersistent: false);

        _logger.LogInformation("New tenant {TenantName} registered with admin {Email}",
            tenant.Name, model.Email);

        // Redirect to onboarding
        return RedirectToAction("Index", "OnboardingWizard");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to register new tenant");
        ModelState.AddModelError("", "Registration failed. Please try again.");
        return View(model);
    }
}
```

---

## 📊 Comparison: ABP vs Your Current System

| Aspect | ABP (Volo.Saas) | Your Current System | What You Need |
|--------|-----------------|---------------------|---------------|
| **Tenant Creation** | ✅ ITenantManager.CreateAsync() | ❌ No tenant creation | ✅ Add TenantCreationService |
| **Admin User** | ✅ Created automatically | ❌ Only creates user, no tenant | ✅ Link user to tenant |
| **TenantUser Link** | ✅ Automatic | ❌ Missing | ✅ Create TenantUser record |
| **Default Workspace** | ✅ Optional seeding | ❌ Missing | ✅ Create default workspace |
| **Edition/Trial** | ✅ Built-in | ❌ Missing | ✅ Add trial fields |
| **Data Seeding** | ✅ TenantDataSeeder | ❌ Missing | ✅ Add seed service |
| **Transaction** | ✅ Atomic | ❌ No transaction | ✅ Wrap in transaction |
| **Error Handling** | ✅ Rollback on error | ❌ Partial creates possible | ✅ Add try/catch/rollback |

---

## ✅ Recommended Implementation (Following ABP Pattern)

### Phase 1: Create TenantCreationService (2 hours)
1. Create TenantCreationService.cs
2. Implement CreateTenantWithAdminAsync()
3. Add transaction handling
4. Add error logging

### Phase 2: Update Registration (1 hour)
1. Add CompanyName field to RegisterViewModel
2. Update Register view
3. Call TenantCreationService in controller
4. Test full flow

### Phase 3: Add Data Seeding (1 hour)
1. Create TenantDataSeeder
2. Seed default roles/permissions
3. Seed sample frameworks (optional)

**Total: 4 hours**

---

## 🎯 Summary: How ABP Does It

1. ✅ **Single Service Call:** `ITenantManager.CreateAsync(name, email, password)`
2. ✅ **Atomic Transaction:** All-or-nothing (rollback on error)
3. ✅ **Complete Setup:** Tenant + User + TenantUser + Workspace + Roles
4. ✅ **Context Switching:** Uses ICurrentTenant to run operations in tenant context
5. ✅ **Data Seeding:** Automatically seeds defaults for new tenant
6. ✅ **Error Handling:** Comprehensive logging and rollback

**You should follow the same pattern!**

---

**Document Date:** 2026-01-13
**Next:** Implement TenantCreationService following ABP pattern
