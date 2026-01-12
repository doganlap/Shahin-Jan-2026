# ABP Framework + Trial + Onboarding Integration

## Overview

Complete integration of **ABP Framework 8.3.6** with existing trial registration and onboarding wizard flows. This ensures all users (new trials and existing) enjoy full ABP features while maintaining the mandatory onboarding wizard completion.

## Key Features

✅ **Dual Tenant Management** - Syncs custom `Tenants` table with ABP `AbpTenants`
✅ **Trial Auto-Login** - Seamless sign-in after trial registration
✅ **Onboarding Enforcement** - All users must complete wizard before workspace access
✅ **ABP Tenant Resolution** - Automatic tenant detection via claims/cookies/headers
✅ **Full ABP Features** - Identity, permissions, audit logging, feature management

---

## Architecture Changes

### 1. ABP Framework Integration (GrcMvcWebModule.cs)

```csharp
[DependsOn(
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountWebModule),
    // ... 8 more modules
)]
public class GrcMvcWebModule : AbpModule
{
    // Multi-tenancy configuration
    // Tenant resolvers (cookie, query, header, user)
    // Identity options
}
```

**Configured Modules:**
- `AbpTenantManagement` - Tenant CRUD
- `AbpIdentity` - User/Role management
- `AbpAccount` - Login/Register
- `AbpPermissionManagement` - Permissions
- `AbpSettingManagement` - Settings
- `AbpAuditLogging` - Audit trail
- `AbpFeatureManagement` - Feature flags

### 2. GrcDbContext Updated

```csharp
public class GrcDbContext : AbpDbContext<GrcDbContext>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ABP module configurations
        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureSettingManagement();
        modelBuilder.ConfigureAuditLogging();
        modelBuilder.ConfigureIdentity();
        modelBuilder.ConfigureFeatureManagement();
        modelBuilder.ConfigureTenantManagement();  // ← Creates AbpTenants table

        // Existing custom configurations...
    }
}
```

### 3. Trial Registration Enhanced (TrialController.cs)

#### New Flow:

```
User submits trial form
    ↓
Create custom Tenant record (Tenants table)
    ↓
Create ABP Tenant record (AbpTenants table) ← NEW!
    ↓
Create ApplicationUser via Identity
    ↓
Add TenantAdmin role
    ↓
Create TenantUser linkage
    ↓
Sign in user + Add TenantId claim ← ENHANCED!
    ↓
Redirect to Onboarding Wizard (mandatory)
```

#### Code Changes:

```csharp
// After creating custom tenant:
if (_abpTenantManager != null)
{
    var abpTenant = await _abpTenantManager.CreateAsync(
        name: tenantSlug,
        id: tenantId  // Same GUID as custom tenant
    );
}

// After sign-in, add TenantId claim:
var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
var identity = claimsPrincipal.Identity as ClaimsIdentity;
identity.AddClaim(new Claim("TenantId", tenantId.ToString()));
await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
```

### 4. Onboarding Enforcement Middleware (NEW!)

**File:** `Middleware/OnboardingEnforcementMiddleware.cs`

**Purpose:** Ensures all authenticated users complete onboarding wizard before accessing workspace.

**How it works:**

1. Runs after `UseAuthentication()` and `UseAuthorization()`
2. Checks if user has `TenantId` claim
3. Looks up tenant's `OnboardingStatus`
4. If status != "COMPLETED", redirects to onboarding wizard
5. Allows access to: `/onboarding`, `/trial`, `/account`, `/api`, static files

**Bypass Routes:**
- `/onboarding/*` - Wizard itself
- `/trial/*` - Trial registration
- `/account/*` - Login/logout
- `/api/*` - REST APIs
- `/health` - Health checks
- Static files (CSS, JS, images)
- Home page (`/`)

**Registered in Program.cs:**

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseOnboardingEnforcement(); // ← NEW!
```

---

## Database Tables

### Custom Tables (Existing)

| Table | Purpose |
|-------|---------|
| `Tenants` | Custom tenant records with trial info |
| `TenantUsers` | User-Tenant linkage |
| `OnboardingWizards` | Onboarding wizard progress |
| `OrganizationProfiles` | Tenant org details |

### ABP Tables (New)

| Table | Purpose |
|-------|---------|
| `AbpTenants` | ABP tenant records (synced with custom) |
| `AbpTenantConnectionStrings` | Per-tenant connection strings |
| `AbpUsers` | ABP user records |
| `AbpRoles` | ABP role definitions |
| `AbpUserRoles` | User-Role mappings |
| `AbpPermissionGrants` | Permission assignments |
| `AbpSettings` | Tenant settings |
| `AbpAuditLogs` | Comprehensive audit trail |
| `AbpFeatures` | Feature flags |

---

## User Flows

### New Trial User Flow

```
1. Visit /trial
2. Fill registration form (org name, email, password)
3. ✅ Tenant created (both Tenants + AbpTenants)
4. ✅ User created via ASP.NET Identity
5. ✅ TenantAdmin role assigned
6. ✅ Auto-login with TenantId claim
7. → Redirect to /t/{tenantSlug}/onboarding/start
8. Complete 12-step onboarding wizard
9. Tenant.OnboardingStatus = "COMPLETED"
10. → Access workspace /t/{tenantSlug}/workspace
```

### Existing User Flow (After Update)

```
1. User logs in via /Account/Login
2. ✅ TenantId claim added (via ClaimsTransformationService)
3. OnboardingEnforcementMiddleware checks Tenant.OnboardingStatus
4. If NOT "COMPLETED":
   → Redirect to /t/{tenantSlug}/onboarding/start
5. User completes onboarding wizard
6. Tenant.OnboardingStatus = "COMPLETED"
7. → Access workspace
```

### Onboarding Wizard Enforcement

**Middleware Logic:**

```csharp
if (tenant.OnboardingStatus != "COMPLETED")
{
    // Redirect to onboarding
    context.Response.Redirect($"/t/{tenant.TenantSlug}/onboarding/start");
    return;
}

// Continue to requested page
await _next(context);
```

**Result:** All workspace/dashboard routes are blocked until onboarding is completed!

---

## Configuration

### appsettings.json (No Changes Required)

ABP uses existing configuration:
- ConnectionStrings (PostgreSQL)
- JWT settings
- Identity settings

### Environment Variables (No Changes Required)

Existing `.env` variables work as-is:
- `DB_HOST`, `DB_PORT`, etc.
- `JWT_SECRET`
- `CLAUDE_API_KEY`

---

## Migration Required

After pulling these changes, run:

```bash
cd src/GrcMvc

# Create migration for ABP tables
dotnet ef migrations add AddAbpFrameworkTables

# Apply migration
dotnet ef database update
```

**Expected New Tables:**
- `AbpTenants` (+ 13 ABP tables)

---

## Benefits

### For New Trial Users

✅ **Instant onboarding** - Auto-login after registration
✅ **ABP tenant tracking** - Appears in ABP tenant management
✅ **Permission system** - Full ABP permissions available
✅ **Audit trail** - All actions logged via ABP audit
✅ **Mandatory wizard** - Cannot skip onboarding

### For Existing Users

✅ **Forced onboarding** - Must complete wizard before workspace access
✅ **Retroactive ABP sync** - Can sync existing tenants to ABP
✅ **Enhanced features** - Gain ABP permissions, settings, features
✅ **Audit history** - Future actions logged

### For Administrators

✅ **ABP tenant management** - Use ABP admin UI for tenant CRUD
✅ **Permission management** - Fine-grained access control
✅ **Feature toggles** - Enable/disable features per tenant
✅ **Audit reports** - Comprehensive audit trail
✅ **Settings management** - Configure tenant settings

---

## Testing Checklist

### New Trial Registration

- [ ] Visit `/trial`
- [ ] Submit registration form
- [ ] Verify auto-login successful
- [ ] Verify redirect to onboarding wizard
- [ ] Complete onboarding wizard (12 steps)
- [ ] Verify `Tenant.OnboardingStatus = "COMPLETED"`
- [ ] Verify can access workspace
- [ ] Verify `AbpTenants` record created

### Existing User Login

- [ ] Login via `/Account/Login`
- [ ] If onboarding not completed, verify redirect to wizard
- [ ] Complete onboarding wizard
- [ ] Verify can access workspace after completion

### Onboarding Bypass Check

- [ ] Try accessing `/t/{slug}/workspace` before onboarding
- [ ] Verify redirect to onboarding wizard
- [ ] Try accessing `/t/{slug}/dashboard` before onboarding
- [ ] Verify redirect to onboarding wizard

### Allowed Routes (No Redirect)

- [ ] Can access `/onboarding/*` (wizard pages)
- [ ] Can access `/trial` (trial registration)
- [ ] Can access `/account/login`
- [ ] Can access `/api/*` (APIs)
- [ ] Can access `/health`
- [ ] Can access static files (CSS, JS)

---

## Known Issues & Limitations

1. **Existing tenants not synced to ABP**
   - New trials auto-sync to `AbpTenants`
   - Existing tenants need manual sync (migration script)
   - **Solution:** Create data migration to sync existing tenants

2. **Onboarding status for old users**
   - Users created before this update may have NULL `OnboardingStatus`
   - **Solution:** Migration sets default to "NOT_STARTED"

3. **ABP Tenant Manager nullable**
   - `ITenantManager?` is nullable in TrialController
   - If ABP not initialized, only custom tenant is created
   - **Solution:** Acceptable - custom tenant still works

---

## Future Enhancements

### Phase 2: Full ABP Integration

- [ ] Migrate custom `Tenants` table to use `AbpTenants` entirely
- [ ] Replace custom permission system with ABP permissions
- [ ] Use ABP settings management for tenant settings
- [ ] Implement ABP feature management for trial/paid tiers

### Phase 3: ABP UI Integration

- [ ] Add ABP admin UI for tenant management
- [ ] Add ABP permission management UI
- [ ] Add ABP audit log viewer
- [ ] Add ABP feature management UI

---

## Rollback Plan

If issues arise, revert by:

1. Remove ABP packages from `GrcMvc.csproj`
2. Revert `GrcDbContext` to inherit from `DbContext`
3. Remove ABP module configurations
4. Remove `OnboardingEnforcementMiddleware`
5. Revert `TrialController` changes
6. Drop ABP tables via migration

**Migration rollback:**
```bash
dotnet ef database update PreviousMigration
dotnet ef migrations remove
```

---

## Support & Documentation

- **ABP Documentation:** https://docs.abp.io/en/abp/latest
- **ABP Tenant Management:** https://docs.abp.io/en/abp/latest/Modules/Tenant-Management
- **Project CLAUDE.md:** Updated with ABP Framework info
- **Issue Tracker:** GitHub Issues

---

**Status:** ✅ Ready for testing
**Last Updated:** 2026-01-12
**Version:** 1.0.0
**Author:** Claude AI Assistant
