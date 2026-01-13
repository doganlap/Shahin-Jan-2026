# Registration Flow Gap Analysis

**Date:** 2026-01-13
**Status:** Implementation vs Specification Review
**Purpose:** Validate current tenant self-registration implementation against detailed specification

---

## 📋 Executive Summary

| Category | Status | Coverage |
|----------|--------|----------|
| **Trial Registration** | ✅ 90% Complete | Tenant + Admin + Auto-login working |
| **Onboarding Entity** | ✅ 100% Complete | Full 12-step wizard entity exists |
| **Fast Start Flow** | ❌ 0% Not Implemented | /onboarding/wizard/fast-start route missing |
| **Onboarding Gate Middleware** | ❌ 0% Not Implemented | No middleware forcing onboarding |
| **Workspace Provisioning** | ⚠️ 50% Partial | Basic workspace created, full provisioning missing |
| **Trial Expiry Enforcement** | ❌ 0% Not Implemented | No middleware blocking expired trials |
| **Integration Tests** | ❌ 0% Not Implemented | No tests for trial-to-onboarding flow |

**Overall Implementation:** 🟡 **60% Complete**

---

## 1. What We Have ✅ (Implemented)

### 1.1 Tenant Self-Registration (ABP Pattern #7077/#8116)

**File:** `src/GrcMvc/Services/Implementations/TenantRegistrationService.cs` (377 lines)

```csharp
public async Task<(Tenant tenant, ApplicationUser adminUser)> CreateTenantWithAdminAsync(
    string companyName,
    string tenantSlug,
    string adminEmail,
    string adminPassword,
    string adminFirstName,
    string adminLastName,
    string edition = "Trial")
```

**What it does:**
1. ✅ Creates Tenant (Company, slug, Status=Active, Edition=Trial)
2. ✅ Sets 14-day trial period (TrialStartedAt, TrialEndsAt, IsTrialExpired=false)
3. ✅ Creates Admin User (Owner role, EmailConfirmed=true)
4. ✅ Creates TenantUser association (links user to tenant) ← **CRITICAL**
5. ✅ Creates Default Workspace
6. ✅ Assigns Owner role to admin
7. ✅ Atomic transaction (all-or-nothing)
8. ✅ Sends welcome email (async, non-blocking)
9. ✅ Logs audit events

**Current Registration Flow:**
```
User fills form → Generate slug → CreateTenantWithAdminAsync()
→ Auto-sign in → Redirect to /Onboarding
```

---

### 1.2 OnboardingWizard Entity

**File:** `src/GrcMvc/Models/Entities/OnboardingWizard.cs` (568 lines)

**✅ Has everything needed:**
- `TenantId` (Guid)
- `CurrentStep` (int 1-12) - Current step in wizard
- `WizardStatus` (string) - NotStarted, InProgress, Completed, Cancelled
- `ProgressPercent` (int 0-100)
- `CompletedSectionsJson` (string)
- `StartedAt` (DateTime?)
- `CompletedAt` (DateTime?)
- `CompletedByUserId` (string)

**12 Sections (A-L):**
- Section A: Organization Identity (13 questions)
- Section B: Assurance Objective (5 questions)
- Section C: Regulatory Applicability (7 questions)
- Section D: Scope Definition (9 questions)
- Section E: Data & Risk Profile (6 questions)
- Section F: Technology Landscape (13 questions)
- Section G: Control Ownership Model (7 questions)
- Section H: Teams, Roles & Access (10 questions)
- Section I: Workflow & Cadence (10 questions)
- Section J: Evidence Standards (7 questions)
- Section K: Baseline & Overlays (3 questions)
- Section L: Go-Live & Success Metrics (6 questions)

**Total:** 96 questions across 12 sections

---

### 1.3 OnboardingWizardController

**File:** `src/GrcMvc/Controllers/OnboardingWizardController.cs`

**Routes:**
- `GET /OnboardingWizard` or `/OnboardingWizard/Index` - Entry point, redirects to current step
- `GET /OnboardingWizard/StepA/{tenantId}` - Section A (Organization Identity)
- `POST /OnboardingWizard/StepA/{tenantId}` - Save Section A
- ... (StepB through StepL)
- `GET /OnboardingWizard/Summary/{tenantId}` - Wizard progress summary

**Features:**
- ✅ Checks authentication
- ✅ Creates wizard if doesn't exist
- ✅ Saves progress per step
- ✅ Validates required fields
- ✅ Shows progress percentage
- ✅ Redirects to current step on resume

---

### 1.4 FREE ABP Packages

**File:** `src/GrcMvc/GrcMvc.csproj`

```xml
<!-- 5 FREE ABP Modules -->
<PackageReference Include="Volo.Abp.TenantManagement.Domain" Version="8.3.5" />
<PackageReference Include="Volo.Abp.TenantManagement.EntityFrameworkCore" Version="8.3.5" />
<PackageReference Include="Volo.Abp.TenantManagement.Application" Version="8.3.5" />
<PackageReference Include="Volo.Abp.FeatureManagement.Domain" Version="8.3.5" />
<PackageReference Include="Volo.Abp.FeatureManagement.EntityFrameworkCore" Version="8.3.5" />
<PackageReference Include="Volo.Abp.FeatureManagement.Web" Version="8.3.5" />
<PackageReference Include="Volo.Abp.AuditLogging.Web" Version="8.3.5" />
<PackageReference Include="Volo.Abp.OpenIddict.Domain" Version="8.3.5" />
<PackageReference Include="Volo.Abp.OpenIddict.EntityFrameworkCore" Version="8.3.5" />
<PackageReference Include="Volo.Abp.Ldap" Version="8.3.5" />
```

**Cost:** $0 (avoided $2,999/year Volo.Saas commercial license)

---

## 2. What We're Missing ❌ (Not Implemented)

### 2.1 Onboarding Gate Middleware ⚠️ **CRITICAL**

**Spec Requirement:**
> "Onboarding is the Firewall" - Middleware should redirect authenticated users to onboarding wizard if not completed.

**Current State:** ❌ **MISSING**

**What's needed:**
```csharp
// File: src/GrcMvc/Middleware/OnboardingGateMiddleware.cs
public class OnboardingGateMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // If authenticated
        if (context.User.Identity.IsAuthenticated)
        {
            // Get tenant
            var tenantId = GetTenantId(context);

            // Check onboarding status
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            // If not completed and not on allowed routes
            if (wizard?.WizardStatus != "Completed" &&
                !IsAllowedRoute(context.Request.Path))
            {
                // Redirect to onboarding wizard
                context.Response.Redirect($"/OnboardingWizard?tenantId={tenantId}");
                return;
            }
        }

        await next(context);
    }

    private bool IsAllowedRoute(string path)
    {
        var allowed = new[] {
            "/OnboardingWizard",
            "/Account",
            "/trial",
            "/static",
            "/api/onboarding"
        };

        return allowed.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
```

**Registration in Program.cs:**
```csharp
// After app.UseAuthentication()
app.UseMiddleware<OnboardingGateMiddleware>();
```

**Impact:** ⚠️ **HIGH PRIORITY**
Without this, users can skip onboarding and access dashboard/features without proper setup.

---

### 2.2 Fast Start Wizard Route ⚠️ **MEDIUM PRIORITY**

**Spec Requirement:**
> Redirect to `/onboarding/wizard/fast-start` after registration

**Current State:** ❌ **MISSING**

**Current redirect:** `/Onboarding` (redirects to comprehensive 12-step wizard)

**Options:**

**Option A: Keep current comprehensive wizard (12 steps)**
- Pros: Complete organizational setup
- Cons: Takes 20-30 minutes to complete
- User impact: May feel overwhelming for trial users

**Option B: Add simplified "Fast Start" wizard (3 steps)**
- Step 1: Minimal org profile (name, sector, primary driver)
- Step 2: Select frameworks/regulators
- Step 3: Create first plan (QuickScan/Comprehensive/Remediation)
- Pros: Faster onboarding (5 minutes)
- Cons: Need to implement new controller/views
- User impact: Faster time-to-value

**Option C: Smart wizard routing**
- Trial users → Fast Start (3 steps)
- Paid users → Comprehensive (12 steps)
- Based on Edition field

**Recommendation:** **Option B** - Add simplified Fast Start wizard

**Implementation files needed:**
```
src/GrcMvc/Controllers/FastStartController.cs (NEW)
src/GrcMvc/Views/FastStart/Step1.cshtml (NEW)
src/GrcMvc/Views/FastStart/Step2.cshtml (NEW)
src/GrcMvc/Views/FastStart/Step3.cshtml (NEW)
src/GrcMvc/Models/DTOs/FastStartDtos.cs (NEW)
```

---

### 2.3 OnboardingWizard Record Creation on Registration

**Spec Requirement:**
> Create OnboardingWizard row with TenantId + CurrentStep=FastStart + Status=InProgress

**Current State:** ⚠️ **PARTIALLY MISSING**

**TenantRegistrationService.cs** does NOT create OnboardingWizard record.

**Fix:**
```csharp
// In CreateTenantWithAdminAsync(), after workspace creation:

// ═══════════════════════════════════════════════════════════
// STEP 8: Create OnboardingWizard Record
// ═══════════════════════════════════════════════════════════
var wizard = new OnboardingWizard
{
    Id = Guid.NewGuid(),
    TenantId = tenant.Id,
    CurrentStep = 1, // Start at Step 1
    WizardStatus = "InProgress",
    ProgressPercent = 0,
    StartedAt = DateTime.UtcNow,
    CreatedDate = DateTime.UtcNow,
    CreatedBy = adminUser.Id,
    IsDeleted = false
};

await _context.OnboardingWizards.AddAsync(wizard);
await _context.SaveChangesAsync();

_logger.LogInformation("Created OnboardingWizard for tenant {TenantId}", tenant.Id);
```

---

### 2.4 Workspace Provisioning on Onboarding Completion

**Spec Requirement:**
> When onboarding completes, provision: Teams, RBAC, RACI, Workflows, Schedules, Assessment Templates, Dashboards

**Current State:** ⚠️ **PARTIALLY IMPLEMENTED**

**What exists:**
- ✅ Basic workspace created during registration
- ⚠️ `IOnboardingProvisioningService` interface exists
- ⚠️ `ITenantOnboardingProvisioner` interface exists
- ❌ Full provisioning NOT triggered on wizard completion

**What's needed:**

**File:** `src/GrcMvc/Services/Implementations/OnboardingCompletionService.cs` (NEW)

```csharp
public async Task ProvisionWorkspaceArtifactsAsync(Guid tenantId)
{
    // 1. Create Teams based on wizard answers (Section H)
    await _teamService.CreateTeamsFromOnboardingAsync(tenantId);

    // 2. Set up RBAC + RACI mappings (Section G & H)
    await _rbacService.ConfigureRolesFromOnboardingAsync(tenantId);

    // 3. Create Workflows based on cadence (Section I)
    await _workflowService.CreateWorkflowsFromOnboardingAsync(tenantId);

    // 4. Generate Assessment Templates based on frameworks (Section C)
    await _assessmentService.GenerateTemplatesFromOnboardingAsync(tenantId);

    // 5. Configure Dashboards + KPIs (Section L)
    await _dashboardService.SetupDashboardsFromOnboardingAsync(tenantId);

    // 6. Set up Evidence Standards (Section J)
    await _evidenceService.ConfigureEvidenceStandardsAsync(tenantId);

    // 7. Create baseline controls (Section K)
    await _controlService.ApplyBaselineControlsAsync(tenantId);
}
```

**Trigger on wizard completion:**
```csharp
// In OnboardingWizardController.StepL() POST action:
if (wizard.CurrentStep == 12 && allStepsCompleted)
{
    wizard.WizardStatus = "Completed";
    wizard.CompletedAt = DateTime.UtcNow;
    wizard.CompletedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    await _context.SaveChangesAsync();

    // ⚠️ PROVISION WORKSPACE ARTIFACTS
    await _onboardingCompletionService.ProvisionWorkspaceArtifactsAsync(wizard.TenantId);

    // Redirect to tenant dashboard
    return RedirectToAction("Index", "Dashboard");
}
```

---

### 2.5 Trial Expiry Enforcement Middleware

**Spec Requirement:**
> Trial expiry blocks operational actions (read-only / payment gate)

**Current State:** ❌ **NOT IMPLEMENTED**

**What's needed:**
```csharp
// File: src/GrcMvc/Middleware/TrialExpiryMiddleware.cs
public class TrialExpiryMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var tenantId = GetTenantId(context);
            var tenant = await _context.Tenants.FindAsync(tenantId);

            // Check if trial expired
            if (tenant.Edition == "Trial" &&
                tenant.TrialEndsAt.HasValue &&
                tenant.TrialEndsAt.Value < DateTime.UtcNow)
            {
                // Mark as expired
                if (!tenant.IsTrialExpired)
                {
                    tenant.IsTrialExpired = true;
                    await _context.SaveChangesAsync();
                }

                // Block write operations
                if (IsWriteOperation(context.Request))
                {
                    context.Response.StatusCode = 402; // Payment Required
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Trial Expired",
                        message = "Your 14-day trial has expired. Please upgrade to continue.",
                        upgradeUrl = "/Subscription/Upgrade"
                    });
                    return;
                }
            }
        }

        await next(context);
    }

    private bool IsWriteOperation(HttpRequest request)
    {
        var method = request.Method;
        return method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH";
    }
}
```

---

### 2.6 Integration Tests

**Spec Requirement:**
> Acceptance tests proving "Day-1 State Exists"

**Current State:** ❌ **NOT IMPLEMENTED**

**Tests needed:**

**File:** `tests/GrcMvc.Tests/Integration/TrialRegistrationFlowTests.cs` (NEW)

```csharp
[Fact]
public async Task Trial_Submit_CreatesTenantAdmin_AndRedirectsToOnboarding()
{
    // Arrange
    var form = new Dictionary<string, string>
    {
        ["CompanyName"] = "Acme Corp",
        ["FirstName"] = "John",
        ["LastName"] = "Doe",
        ["Email"] = "john@acme.test",
        ["Password"] = "Test123!@#$%",
        ["ConfirmPassword"] = "Test123!@#$%"
    };

    // Act
    var response = await _client.PostAsync("/Account/Register",
        new FormUrlEncodedContent(form));

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    response.Headers.Location.ToString().Should().Contain("/Onboarding");

    // Verify database state
    var tenant = await _context.Tenants
        .FirstOrDefaultAsync(t => t.TenantSlug == "acme-corp");
    tenant.Should().NotBeNull();
    tenant.Edition.Should().Be("Trial");
    tenant.TrialStartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    tenant.TrialEndsAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(5));
}

[Fact]
public async Task OnboardingGate_RedirectsUnauthenticatedUsers()
{
    var response = await _client.GetAsync("/Dashboard");
    response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    response.Headers.Location.ToString().Should().Contain("/OnboardingWizard");
}

[Fact]
public async Task OnboardingCompletion_ProvisionsWorkspaceArtifacts()
{
    // Create tenant with completed onboarding
    var tenant = await CreateTenantWithCompletedOnboarding();

    // Verify artifacts exist
    _context.Teams.Any(t => t.TenantId == tenant.Id).Should().BeTrue();
    _context.Workflows.Any(w => w.TenantId == tenant.Id).Should().BeTrue();
    _context.AssessmentTemplates.Any(a => a.TenantId == tenant.Id).Should().BeTrue();
}

[Fact]
public async Task TrialExpiry_BlocksWriteOperations()
{
    var tenant = await CreateTenantWithExpiredTrial();
    var client = await CreateAuthenticatedClient(tenant);

    var response = await client.PostAsJsonAsync("/api/assessments", new { Name = "Test" });

    response.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);
}
```

---

## 3. Comparison: Current vs Spec

### 3.1 Trial Registration Flow

| Step | Spec | Current | Status |
|------|------|---------|--------|
| 1. User submits trial form | /trial/register | /Account/Register or /Account/Signup | ✅ Equivalent |
| 2. Server validates | ✅ | ✅ | ✅ Complete |
| 3. Create tenant + admin | ABP ITenantAppService.CreateAsync | TenantRegistrationService.CreateTenantWithAdminAsync | ✅ Complete |
| 4. Set trial period | TrialStart, TrialEnd | TrialStartedAt, TrialEndsAt | ✅ Complete |
| 5. Create OnboardingWizard | CurrentStep=FastStart, Status=InProgress | ❌ Not created | ❌ Missing |
| 6. Auto-login admin | ✅ | ✅ | ✅ Complete |
| 7. Redirect | /onboarding/wizard/fast-start | /Onboarding (comprehensive wizard) | ⚠️ Partial |

**Gap Summary:**
- ❌ OnboardingWizard record not created during registration
- ⚠️ Redirects to comprehensive wizard (12 steps) instead of fast-start (3 steps)

---

### 3.2 Onboarding Gate (Middleware)

| Feature | Spec | Current | Status |
|---------|------|---------|--------|
| Middleware exists | ✅ Required | ❌ Missing | ❌ Not implemented |
| Checks authenticated | ✅ | N/A | ❌ |
| Checks onboarding status | ✅ | N/A | ❌ |
| Redirects if not completed | ✅ | N/A | ❌ |
| Allowed routes | /OnboardingWizard/*, /Account/*, /trial/*, /static/* | N/A | ❌ |

**Gap Summary:**
- ❌ No middleware enforcing onboarding completion

**Impact:** Users can skip onboarding and access dashboard without proper setup.

---

### 3.3 Workspace Provisioning

| Artifact | Spec | Current | Status |
|----------|------|---------|--------|
| Teams | ✅ Provision on completion | ❌ Not implemented | ❌ |
| RBAC + RACI | ✅ Configure from wizard | ❌ Not implemented | ❌ |
| Workflows | ✅ Create based on cadence | ❌ Not implemented | ❌ |
| Assessment Templates | ✅ Generate from frameworks | ❌ Not implemented | ❌ |
| Dashboards + KPIs | ✅ Enable | ❌ Not implemented | ❌ |
| Evidence Standards | ✅ Configure | ❌ Not implemented | ❌ |
| Baseline Controls | ✅ Apply overlays | ❌ Not implemented | ❌ |
| Basic Workspace | ✅ | ✅ Created during registration | ✅ |

**Gap Summary:**
- ✅ Basic workspace created (name, tenant link, status)
- ❌ Full provisioning (teams, workflows, templates, dashboards) NOT implemented

---

### 3.4 Trial Expiry

| Feature | Spec | Current | Status |
|---------|------|---------|--------|
| Trial fields exist | TrialStart, TrialEnd | TrialStartedAt, TrialEndsAt, IsTrialExpired | ✅ Complete |
| Middleware checks expiry | ✅ | ❌ Missing | ❌ |
| Block write operations | 402 Payment Required | N/A | ❌ |
| Allow read operations | ✅ | N/A | ❌ |
| Upgrade prompt | /Subscription/Upgrade | N/A | ❌ |

**Gap Summary:**
- ✅ Database fields for trial tracking exist
- ❌ No enforcement middleware

---

## 4. Implementation Priority

### P0 (Critical - Required for MVP)

1. **Create OnboardingWizard on Registration** ⏱️ 30 min
   - Modify `TenantRegistrationService.CreateTenantWithAdminAsync()`
   - Add wizard creation after workspace
   - 10 lines of code

2. **Onboarding Gate Middleware** ⏱️ 2 hours
   - Create `OnboardingGateMiddleware.cs`
   - Register in `Program.cs`
   - 50 lines of code

### P1 (High - Recommended for Launch)

3. **Fast Start Wizard** ⏱️ 8 hours
   - Create `FastStartController.cs`
   - Create 3 views (Step1, Step2, Step3)
   - Create DTOs
   - 300 lines of code

4. **Onboarding Completion Provisioning** ⏱️ 16 hours
   - Create `OnboardingCompletionService.cs`
   - Implement 7 provisioning methods
   - Integrate with wizard controller
   - 500 lines of code

### P2 (Medium - Post-Launch)

5. **Trial Expiry Middleware** ⏱️ 4 hours
   - Create `TrialExpiryMiddleware.cs`
   - Create upgrade page
   - 100 lines of code

6. **Integration Tests** ⏱️ 8 hours
   - Create test fixtures
   - Write 4 test classes
   - 400 lines of code

---

## 5. Recommendations

### Immediate Actions (Today)

1. **✅ Fix OnboardingWizard Creation**
   - Add wizard creation to `TenantRegistrationService`
   - Ensures wizard exists for all new tenants

2. **✅ Implement Onboarding Gate Middleware**
   - Prevent users from skipping onboarding
   - Force completion before accessing features

### Short Term (This Week)

3. **Add Fast Start Wizard (Optional)**
   - Decision: Keep comprehensive 12-step wizard OR add simplified 3-step Fast Start
   - If keeping comprehensive: Update redirect message to set expectations
   - If adding Fast Start: Implement new controller + 3 views

4. **Implement Basic Provisioning**
   - At minimum: Create default assessment template on completion
   - Full provisioning can be phased

### Medium Term (Next Sprint)

5. **Trial Expiry Enforcement**
   - Add middleware blocking write operations
   - Create upgrade/payment flow

6. **Integration Tests**
   - Test trial registration flow
   - Test onboarding gate
   - Test workspace provisioning

---

## 6. Code Changes Needed

### File 1: TenantRegistrationService.cs (10 lines added)

```csharp
// After workspace creation (line ~200):

// ═══════════════════════════════════════════════════════════
// STEP 8: Create OnboardingWizard Record
// ═══════════════════════════════════════════════════════════
var wizard = new OnboardingWizard
{
    Id = Guid.NewGuid(),
    TenantId = tenant.Id,
    CurrentStep = 1,
    WizardStatus = "InProgress",
    ProgressPercent = 0,
    StartedAt = DateTime.UtcNow,
    CreatedDate = DateTime.UtcNow,
    CreatedBy = adminUser.Id
};

await _context.OnboardingWizards.AddAsync(wizard);
await _context.SaveChangesAsync();

_logger.LogInformation("Created OnboardingWizard for tenant {TenantId}", tenant.Id);
```

### File 2: OnboardingGateMiddleware.cs (NEW - 80 lines)

See section 2.1 for full implementation.

### File 3: Program.cs (1 line added)

```csharp
// After app.UseAuthentication() and before app.UseAuthorization():
app.UseMiddleware<OnboardingGateMiddleware>();
```

---

## 7. Test Plan

### Manual Testing

1. **Trial Registration**
   - Navigate to `/Account/Signup`
   - Fill: Company="Test Corp", Name="John Doe", Email="john@test.com", Password
   - Click Register
   - ✅ Should redirect to `/OnboardingWizard`
   - ✅ Database check: Tenant, User, TenantUser, Workspace, OnboardingWizard created

2. **Onboarding Gate**
   - Login as new user (not completed onboarding)
   - Try accessing `/Dashboard`
   - ✅ Should redirect to `/OnboardingWizard`
   - Complete wizard
   - Try accessing `/Dashboard` again
   - ✅ Should allow access

3. **Trial Expiry**
   - Create tenant with `TrialEndsAt = DateTime.UtcNow.AddDays(-1)`
   - Login
   - Try creating assessment
   - ✅ Should return 402 Payment Required

---

## 8. Conclusion

### What Works ✅
- Tenant self-registration following ABP pattern
- Complete OnboardingWizard entity (96 questions, 12 sections)
- Comprehensive wizard controller and views
- Auto-sign in after registration
- FREE ABP packages integrated

### Critical Gaps ❌
- OnboardingWizard record not created during registration
- No onboarding gate middleware (users can skip onboarding)
- No "Fast Start" simplified wizard
- No workspace provisioning on completion
- No trial expiry enforcement
- No integration tests

### Estimated Effort
- **P0 (Critical):** 2.5 hours
- **P1 (High):** 24 hours
- **P2 (Medium):** 12 hours
- **Total:** ~38 hours (1 week for 1 developer)

### Next Steps
1. Review this gap analysis
2. Decide: Keep comprehensive wizard OR add Fast Start
3. Prioritize: P0 → P1 → P2
4. Implement missing components
5. Test end-to-end flow
6. Deploy to staging

---

**Document Version:** 1.0
**Last Updated:** 2026-01-13
**Author:** Claude Code Agent
**Status:** Ready for Review
