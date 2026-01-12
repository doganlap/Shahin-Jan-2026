# /SignupNew Trial Registration Implementation

## Overview

New trial registration endpoint using **Razor Pages** at route `/SignupNew`, leveraging ABP Framework's built-in `ITenantManager` for tenant creation. This is an alternative to the existing `/trial` controller route.

## Architecture

### Route Structure
```
/SignupNew → Pages/SignupNew/Index.cshtml (Razor Page)
/trial     → Controllers/TrialController.cs (MVC Controller) [Existing]
```

Both routes are functional and can coexist. Use `/SignupNew` for ABP-first approach.

### Flow Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│ 1. User visits /SignupNew                                        │
│    • Renders clean registration form                             │
│    • Fields: Company Name, Full Name (opt), Email, Password     │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 2. Form Submission (OnPostAsync)                                 │
│    • Validates input (ModelState)                                │
│    • Sanitizes company name → slug (removes non-alphanumeric)   │
│    • Example: "Acme Corp!" → "acmecorp"                          │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 3. Generate Unique Slug                                          │
│    • Check if slug exists in AbpTenants + Tenants tables        │
│    • If exists, append suffix: acmecorp → acmecorp2 → acmecorp3│
│    • Safety limit: 1000 attempts                                 │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 4. Create ABP Tenant (ITenantManager)                            │
│    • Call: _tenantManager.CreateAsync(slug, tenantId)           │
│    • Inserts into AbpTenants table                               │
│    • ABP handles tenant isolation setup                          │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 5. Create Custom Tenant (GRC-specific)                           │
│    • Same tenantId as ABP tenant                                 │
│    • Fields: IsTrial, TrialStartsAt, TrialEndsAt, etc.          │
│    • Inserts into Tenants table (custom)                         │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 6. Create Admin User (within tenant context)                     │
│    • Uses ICurrentTenant.Change(tenantId) for isolation         │
│    • Creates ApplicationUser via UserManager                     │
│    • Auto-confirms email in non-production                       │
│    • Assigns "TenantAdmin" role                                  │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 7. Create TenantUser Linkage                                     │
│    • Links UserId ↔ TenantId                                     │
│    • Required for custom tenant context resolution              │
│    • Inserts into TenantUsers table                              │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 8. Auto-Login with TenantId Claim                                │
│    • SignInManager.SignInAsync(user, isPersistent: true)        │
│    • Add TenantId claim to ClaimsPrincipal                       │
│    • Re-sign in with updated claims                              │
└──────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────┐
│ 9. Redirect to Onboarding Wizard (Mandatory)                     │
│    • RedirectToAction("Start", "Onboarding", { tenantSlug })   │
│    • User must complete 12-step onboarding                       │
│    • Workspace access blocked until OnboardingStatus=COMPLETED  │
└──────────────────────────────────────────────────────────────────┘
```

---

## File Structure

```
src/GrcMvc/
├── Pages/
│   └── SignupNew/
│       ├── Index.cshtml           ← Razor view (form UI)
│       └── Index.cshtml.cs        ← Page model (backend logic)
├── Program.cs                     ← Already configured (AddRazorPages, MapRazorPages)
└── GrcMvcWebModule.cs             ← ABP module configuration
```

---

## Key Features

### 1. Slug Generation Logic

**Sanitization:**
```csharp
Input: "Acme Corp & Co.!"
Output: "acmecorp"

Regex: [^a-z0-9] removed
Max length: 50 characters
```

**Uniqueness Check:**
```csharp
Check: AbpTenants.Name == "acmecorp"
Check: Tenants.TenantSlug == "acmecorp"

If exists:
  acmecorp → acmecorp2 → acmecorp3 → ... → acmecorp999

Safety: Max 1000 attempts (throws exception if exhausted)
```

### 2. Dual Tenant Creation

| Table | Purpose | Primary Key |
|-------|---------|-------------|
| `AbpTenants` | ABP tenant management | Same `tenantId` |
| `Tenants` | GRC-specific fields (trial, billing) | Same `tenantId` |

**Why both?**
- ABP expects `AbpTenants` for its tenant resolution system
- Custom `Tenants` table stores GRC-specific fields (trial dates, billing status, onboarding status)

### 3. Tenant Context Isolation

```csharp
using (_currentTenant.Change(tenantId))
{
    // All operations here are scoped to this tenant
    var user = await _userManager.CreateAsync(...);
    await _userManager.AddToRoleAsync(user, "TenantAdmin");
}
```

**Benefits:**
- EF Core query filters automatically applied
- User/role records correctly associated with tenant
- ABP's multi-tenancy middleware handles resolution

### 4. Error Handling & Rollback

```csharp
try {
    // Create tenant
    // Create user
} catch (Exception ex) {
    // Rollback: Delete ABP tenant + custom tenant
    await _tenantRepository.DeleteAsync(abpTenant);
    _context.Tenants.Remove(customTenant);
    await _context.SaveChangesAsync();

    return Page(); // Show error to user
}
```

### 5. UI/UX Features

- **Modern Design**: Bootstrap 5, gradient background, card-based layout
- **Validation**: Client-side + server-side validation
- **Loading States**: Button disabled during submission with spinner
- **Password Toggle**: Show/hide password visibility
- **Responsive**: Mobile-friendly design
- **Accessibility**: ARIA labels, semantic HTML

---

## Testing

### Manual Test Scenarios

#### 1. Happy Path (New Trial)
```
Steps:
1. Navigate to http://localhost:5010/SignupNew
2. Fill form:
   - Company: "Acme Corporation"
   - Full Name: "John Doe"
   - Email: "john@acme.com"
   - Password: "SecurePass123!"
   - Accept Terms: ✓
3. Click "Start Free Trial"

Expected:
✅ User auto-logged in
✅ Redirected to /t/acmecorporation/onboarding/start
✅ AbpTenants has record with Name="acmecorporation"
✅ Tenants has record with TenantSlug="acmecorporation"
✅ TenantUsers links user to tenant
✅ User has "TenantAdmin" role
✅ OnboardingEnforcementMiddleware blocks workspace access
```

#### 2. Duplicate Company Name
```
Steps:
1. Register "Acme Corp" → slug: "acmecorp"
2. Register "Acme Corp" again

Expected:
✅ Second tenant gets slug "acmecorp2"
✅ Both tenants exist independently
✅ No database constraint violations
```

#### 3. Duplicate Email (Same Tenant)
```
Steps:
1. Register user1@acme.com for "Acme Corp"
2. Register user1@acme.com for "Acme Corp" again

Expected:
❌ Error: "An account with this email already exists."
✅ No duplicate users created
```

#### 4. Duplicate Email (Different Tenant)
```
Steps:
1. Register admin@example.com for "Acme Corp"
2. Register admin@example.com for "Beta Inc"

Expected:
✅ Two separate users (different tenants)
✅ ABP multi-tenancy isolates users
```

#### 5. Invalid Input
```
Test Cases:
- Empty company name → Validation error
- Invalid email format → Validation error
- Short password (< 12 chars) → Validation error
- Terms not accepted → Validation error

Expected:
✅ Form validation prevents submission
✅ User-friendly error messages displayed
```

#### 6. Special Characters in Company Name
```
Input: "Acme & Co.! (2024)"
Expected Slug: "acmeco2024"

Verification:
✅ All non-alphanumeric characters removed
✅ Slug is valid and unique
```

### Database Verification

After successful registration, verify:

**AbpTenants table:**
```sql
SELECT Id, Name, CreationTime
FROM "AbpTenants"
WHERE Name = 'acmecorporation';

Expected:
Id: {guid}
Name: acmecorporation
CreationTime: {timestamp}
```

**Tenants table (custom):**
```sql
SELECT Id, TenantSlug, OrganizationName, IsTrial, OnboardingStatus
FROM "Tenants"
WHERE TenantSlug = 'acmecorporation';

Expected:
Id: {same guid as AbpTenants}
TenantSlug: acmecorporation
OrganizationName: Acme Corporation
IsTrial: true
OnboardingStatus: NOT_STARTED
```

**AspNetUsers table:**
```sql
SELECT Id, Email, EmailConfirmed
FROM "AspNetUsers"
WHERE Email = 'john@acme.com';

Expected:
Id: {guid}
Email: john@acme.com
EmailConfirmed: true (in dev), false (in prod)
```

**TenantUsers table:**
```sql
SELECT TenantId, UserId, RoleCode, Status
FROM "TenantUsers"
WHERE UserId = {user_id_from_above};

Expected:
TenantId: {tenant_id}
UserId: {user_id}
RoleCode: TenantAdmin
Status: Active
```

**AspNetUserRoles table:**
```sql
SELECT ur.UserId, r.Name
FROM "AspNetUserRoles" ur
JOIN "AspNetRoles" r ON ur.RoleId = r.Id
WHERE ur.UserId = {user_id};

Expected:
UserId: {user_id}
Name: TenantAdmin
```

---

## Configuration

### Program.cs (Already Configured)

```csharp
// Line ~1300: Razor Pages enabled
builder.Services.AddRazorPages();

// Line ~1646: Razor Pages mapped
app.MapRazorPages();
```

### GrcMvcWebModule.cs (Already Configured)

```csharp
Configure<AbpMultiTenancyOptions>(options =>
{
    options.IsEnabled = true;
});

Configure<AbpTenantResolveOptions>(options =>
{
    options.TenantResolvers.Add(new CurrentUserTenantResolveContributor());
    options.TenantResolvers.Add(new CookieTenantResolveContributor());
    // ...
});
```

---

## Differences from `/trial` Controller

| Feature | `/SignupNew` (Razor Page) | `/trial` (Controller) |
|---------|---------------------------|----------------------|
| **Technology** | Razor Pages | MVC Controller |
| **Tenant Creation** | `ITenantManager` (ABP) first, then custom | Custom `Tenant` first, then ABP (optional) |
| **Slug Generation** | Robust unique check (dual table) | Basic uniqueness with timestamp fallback |
| **Error Handling** | Rollback ABP + custom tenant | Transaction rollback |
| **UI** | Modern card-based form | Standard form |
| **Validation** | Client + server-side | Server-side |
| **Route** | `/SignupNew` | `/trial` |

**Recommendation:** Use `/SignupNew` for new implementations (ABP-first approach).

---

## Onboarding Enforcement

After successful registration, the user is redirected to:
```
/t/{tenantSlug}/onboarding/start
```

**OnboardingEnforcementMiddleware** ensures:
- ✅ User cannot access workspace until onboarding completed
- ✅ All workspace routes blocked (except onboarding, account, API)
- ✅ Automatic redirect if user tries to access workspace directly

---

## Integration with Existing System

### Compatible With:
- ✅ Existing `/trial` controller
- ✅ `OnboardingEnforcementMiddleware`
- ✅ `ClaimsTransformationService` (adds TenantId claim)
- ✅ Custom tenant management
- ✅ ABP tenant management

### No Conflicts:
- Both `/SignupNew` and `/trial` can coexist
- Routes are independent
- Both create tenants in same tables (dual-write)

---

## Security Considerations

### 1. Email Confirmation
```csharp
EmailConfirmed = !HttpContext.RequestServices
    .GetRequiredService<IWebHostEnvironment>()
    .IsProduction()
```
- **Development:** Auto-confirmed (easier testing)
- **Production:** Requires email verification (secure)

### 2. Password Policy
- Minimum 12 characters (enforced via validation)
- ASP.NET Identity password policies apply (configured in Program.cs)

### 3. CSRF Protection
```html
@Html.AntiForgeryToken()
```
- Anti-forgery token included in form
- ASP.NET Core validates token on POST

### 4. Rate Limiting
- Existing rate limiting middleware applies
- Protects against brute-force registration attacks

---

## Troubleshooting

### Issue: "ITenantManager not registered"
**Cause:** ABP module not initialized
**Solution:** Ensure `GrcMvcWebModule` is loaded in Program.cs

### Issue: "Slug already exists" (duplicate error)
**Cause:** Race condition (two simultaneous registrations)
**Solution:** Suffix logic handles this (acmecorp → acmecorp2)

### Issue: "User creation failed"
**Cause:** Password policy violation or duplicate email
**Solution:** Check error messages, verify password meets requirements

### Issue: "Redirect loop after login"
**Cause:** OnboardingStatus not set correctly
**Solution:** Verify custom Tenant record has `OnboardingStatus = "NOT_STARTED"`

### Issue: "TenantId claim missing"
**Cause:** ClaimsTransformationService not running
**Solution:** Code explicitly adds claim during signup (line 192-200)

---

## Future Enhancements

### Phase 1 (Current Implementation)
- ✅ Basic trial registration via /SignupNew
- ✅ ABP tenant creation
- ✅ Auto-login with TenantId claim
- ✅ Onboarding wizard redirect

### Phase 2 (Future)
- [ ] Email verification workflow
- [ ] Captcha integration (prevent bots)
- [ ] Social login (Google, Microsoft)
- [ ] Invite-only trial codes

### Phase 3 (Future)
- [ ] Trial usage analytics
- [ ] Trial-to-paid conversion tracking
- [ ] Welcome email automation
- [ ] Onboarding progress tracking

---

## API Reference

### ITenantManager
```csharp
Task<Tenant> CreateAsync(string name, Guid id);
```
**Description:** Creates ABP tenant with specified name and ID.

### ITenantRepository
```csharp
Task<Tenant?> FindByNameAsync(string name);
Task InsertAsync(Tenant tenant);
Task DeleteAsync(Tenant tenant);
```
**Description:** Repository for querying/manipulating ABP tenants.

### ICurrentTenant
```csharp
IDisposable Change(Guid? tenantId);
```
**Description:** Temporarily switches tenant context (for scoped operations).

---

## Testing Checklist

- [ ] Navigate to /SignupNew - page loads
- [ ] Submit empty form - validation errors shown
- [ ] Submit valid form - registration succeeds
- [ ] Verify ABP tenant created
- [ ] Verify custom tenant created (with trial fields)
- [ ] Verify user created and assigned TenantAdmin role
- [ ] Verify TenantUser linkage created
- [ ] Verify auto-login successful
- [ ] Verify redirect to onboarding wizard
- [ ] Complete onboarding wizard - status updated
- [ ] Try accessing workspace before onboarding - blocked
- [ ] Complete onboarding - workspace accessible
- [ ] Register duplicate company name - slug gets suffix
- [ ] Check database for all records

---

## Conclusion

The `/SignupNew` Razor Page provides a clean, ABP-first approach to trial registration with:
- ✅ Modern UI/UX
- ✅ Robust slug generation
- ✅ ABP Framework integration
- ✅ Dual tenant management (ABP + custom)
- ✅ Auto-login with tenant context
- ✅ Mandatory onboarding enforcement
- ✅ Production-ready error handling

**Status:** ✅ Fully implemented and ready for testing
**Last Updated:** 2026-01-12
**Route:** http://localhost:5010/SignupNew
