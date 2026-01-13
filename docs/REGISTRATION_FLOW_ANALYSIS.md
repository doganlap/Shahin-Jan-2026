# Registration Flow Analysis - Current vs Needed

**Date:** 2026-01-13
**Status:** ⚠️ INCOMPLETE - Needs decision before implementation
**Question:** What should /Account/Register actually create?

---

## ❌ CURRENT REGISTRATION FLOW (INCOMPLETE!)

**File:** `src/GrcMvc/Controllers/AccountController.cs` (Line 540-615)

### What It Currently Does:

```
User visits /Account/Register or /Account/Signup
  ↓
Fills form: Email, Password, FirstName, LastName, Department
  ↓
System creates:
  1. ✅ User account (ApplicationUser)
  2. ✅ Assigns "User" role
  3. ✅ Sends welcome email
  4. ✅ Auto-login
  5. ✅ Redirect to Home
```

### What It DOES NOT Do:

```
❌ Does NOT create Tenant
❌ Does NOT create TenantUser association
❌ Does NOT set up Workspace
❌ Does NOT assign Edition (Free/Trial/Professional)
❌ Does NOT make user "Owner" or "Admin"
❌ Does NOT start Onboarding Wizard
❌ Does NOT provision tenant with default data
```

### Current Problem:

**User is created but has NO TENANT!**

This means:
- User can't create assessments (needs TenantId)
- User can't create controls (needs TenantId)
- User can't access any features (all require tenant)
- User just sees empty dashboard
- Multi-tenant system is broken!

---

## 🤔 DECISION NEEDED: What Should Registration Create?

### Option 1: User + Tenant + Owner (Recommended for SaaS) ⭐

**Best for:** Public SaaS where each company gets their own tenant

```
User signs up
  ↓
System creates:
  1. User account (ApplicationUser)
  2. Tenant (with user's company name)
  3. TenantUser association (User = Owner)
  4. Default Workspace
  5. Assign "Owner" role (full admin rights)
  6. Set Edition = "Trial" (14-day trial)
  7. Send welcome email
  8. Auto-login
  9. Redirect to Onboarding Wizard
```

**When to use:**
- ✅ B2B SaaS (each company = tenant)
- ✅ Users want immediate access
- ✅ Self-service signup
- ✅ Trial-first approach

**Examples:** Slack, Asana, Trello, Notion

---

### Option 2: User Only (Needs Invitation)

**Best for:** Enterprise where admin invites users

```
User signs up
  ↓
System creates:
  1. User account (ApplicationUser)
  2. Assigns "User" role (basic)
  3. Send welcome email
  4. Auto-login
  5. Show "Waiting for invitation" page
  ↓
Admin invites user to existing tenant
  ↓
User can now access tenant
```

**When to use:**
- ✅ Enterprise software (controlled access)
- ✅ Users must be invited by admin
- ✅ No self-service tenant creation
- ✅ Centralized control

**Examples:** Jira (enterprise), Salesforce (enterprise)

---

### Option 3: User + Join Request

**Best for:** Community platform where users request access

```
User signs up
  ↓
System creates:
  1. User account (ApplicationUser)
  2. Assigns "User" role
  3. Send welcome email
  4. Show "Browse public tenants" page
  ↓
User requests to join a tenant
  ↓
Tenant admin approves
  ↓
User can access tenant
```

**When to use:**
- ✅ Platform with existing organizations
- ✅ Users browse and request to join
- ✅ Approval workflow needed

**Examples:** GitHub (join organization), LinkedIn (join company page)

---

### Option 4: User + Auto-Join Default Tenant

**Best for:** Single organization with multiple users

```
User signs up
  ↓
System creates:
  1. User account (ApplicationUser)
  2. Automatically join DEFAULT tenant
  3. TenantUser association (User = Member)
  4. Assigns "User" role
  5. Send welcome email
  6. Auto-login
  7. Redirect to Dashboard
```

**When to use:**
- ✅ Single organization (not multi-tenant SaaS)
- ✅ All users belong to same company
- ✅ Simple access control

**Examples:** Internal company tools

---

## 📊 Comparison Table

| Aspect | Option 1: Tenant + Owner | Option 2: User Only | Option 3: Join Request | Option 4: Auto-Join |
|--------|-------------------------|---------------------|------------------------|---------------------|
| **Creates Tenant?** | ✅ Yes, automatically | ❌ No | ❌ No | ❌ No |
| **User Role** | Owner (admin) | User (basic) | User (basic) | User (member) |
| **Immediate Access?** | ✅ Yes, full access | ❌ No, needs invite | ❌ No, needs approval | ✅ Yes, member access |
| **Trial Support?** | ✅ Yes, 14-day trial | ❌ N/A | ❌ N/A | ❌ N/A |
| **SaaS Pricing?** | ✅ Yes, per tenant | ❌ No | ❌ No | ❌ No |
| **Best For** | B2B SaaS | Enterprise | Community | Single org |
| **Examples** | Slack, Notion | Salesforce | GitHub | Internal tools |

---

## 🎯 RECOMMENDATION: Option 1 (Tenant + Owner)

### Why Option 1?

Your system is clearly designed as **multi-tenant SaaS**:
- ✅ You have Tenant table
- ✅ You have TenantUser table
- ✅ You have Workspace table
- ✅ You have Edition field (Free/Trial/Professional)
- ✅ You have Onboarding Wizard (12 steps)
- ✅ Landing page says "Start Free Trial"

**Every feature requires a tenant!**

### What Should Happen:

```
User visits landing page → Clicks "Start Free Trial"
  ↓
Goes to /Account/Signup
  ↓
Fills form:
  - Company Name (NEW field needed!)
  - Email
  - Password
  - First Name
  - Last Name
  - Department (optional)
  ↓
System creates:
  1. Tenant (Name = Company Name, Edition = "Trial")
  2. User account (ApplicationUser)
  3. TenantUser (UserId, TenantId, Role = "Owner")
  4. Default Workspace
  5. Assign "Owner" role to user
  6. Set trial: TrialEndsAt = Now + 14 days
  7. Send welcome email (with trial info)
  8. Auto-login
  9. Redirect to /Onboarding/Step1
  ↓
User starts 12-step onboarding wizard
  ↓
User has fully provisioned tenant with trial
```

---

## 📝 REQUIRED CHANGES (If Option 1 Chosen)

### 1. Add CompanyName to RegisterViewModel

**File:** `src/GrcMvc/Models/ViewModels/RegisterViewModel.cs`

```csharp
public class RegisterViewModel
{
    [Required]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;  // NEW

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // ... rest
}
```

### 2. Add CompanyName to Register View

**File:** `src/GrcMvc/Views/Account/Register.cshtml`

Add field for company name (first field in form).

### 3. Update Register Controller Action

**File:** `src/GrcMvc/Controllers/AccountController.cs`

Replace lines 554-605 with:

```csharp
// Create user
var user = new ApplicationUser { ... };
await _userManager.CreateAsync(user, model.Password);

// Create tenant
var tenant = new Tenant
{
    Name = model.CompanyName,
    Edition = "Trial",
    TrialStartedAt = DateTime.UtcNow,
    TrialEndsAt = DateTime.UtcNow.AddDays(14),
    IsTrialExpired = false,
    IsActive = true
};
await _context.Tenants.AddAsync(tenant);
await _context.SaveChangesAsync();

// Create TenantUser association
var tenantUser = new TenantUser
{
    TenantId = tenant.Id,
    UserId = user.Id,
    Role = "Owner",
    Status = "Active"
};
await _context.TenantUsers.AddAsync(tenantUser);
await _context.SaveChangesAsync();

// Assign Owner role
await _userManager.AddToRoleAsync(user, "Owner");

// Create default workspace
var workspace = new Workspace
{
    TenantId = tenant.Id,
    Name = "Main Workspace",
    IsDefault = true
};
await _context.Workspaces.AddAsync(workspace);
await _context.SaveChangesAsync();

// Send welcome email with trial info
await SendTrialWelcomeEmail(user.Email, tenant.TrialEndsAt.Value);

// Auto-login
await _signInManager.SignInAsync(user, isPersistent: false);

// Redirect to onboarding
return RedirectToAction("Index", "OnboardingWizard");
```

**Estimated effort:** 2 hours

---

### 4. Add Trial Fields to Tenant Table

**Migration:**
```sql
ALTER TABLE Tenants
    ADD COLUMN TrialStartedAt TIMESTAMP NULL,
    ADD COLUMN TrialEndsAt TIMESTAMP NULL,
    ADD COLUMN IsTrialExpired BOOLEAN DEFAULT FALSE;
```

**Estimated effort:** 15 minutes

---

## ⚠️ QUESTIONS TO ANSWER BEFORE CODING

### Q1: Which option do you want?

- [ ] **Option 1: Tenant + Owner** (Recommended for SaaS) ⭐
- [ ] **Option 2: User Only** (Enterprise with invitations)
- [ ] **Option 3: Join Request** (Community platform)
- [ ] **Option 4: Auto-Join Default** (Single organization)

### Q2: If Option 1, should trial be automatic?

- [ ] Yes - Everyone gets 14-day trial automatically
- [ ] No - Free plan only, upgrade required for trial
- [ ] Optional - User chooses Free or Trial during signup

### Q3: What happens after 14-day trial?

- [ ] Auto-downgrade to Free plan (keeps data)
- [ ] Lock account, require upgrade (force payment)
- [ ] Send warning, extend 7 days grace period
- [ ] Contact sales team manually

### Q4: Required fields for signup?

Current:
- Email
- Password
- First Name
- Last Name
- Department (optional)

If Option 1, add:
- [ ] Company Name (required)
- [ ] Company Size (optional)
- [ ] Industry (optional)
- [ ] Phone Number (optional)

### Q5: What role should first user get?

- [ ] **Owner** - Full admin rights (recommended)
- [ ] **Admin** - Limited admin rights
- [ ] **User** - Basic user rights

### Q6: Should onboarding be mandatory?

- [ ] Yes - Force user through 12-step wizard
- [ ] No - Skip to dashboard, onboarding optional
- [ ] Partial - Only critical steps (steps 1-3)

### Q7: Email confirmation?

Current: Auto-confirmed in dev, requires confirmation in production

- [ ] Keep as is
- [ ] Always require confirmation (more secure)
- [ ] Never require confirmation (easier signup)

---

## 🚫 DO NOT CODE UNTIL DECIDED

**Current Status:** ⚠️ Waiting for decisions on Q1-Q7

Once decided, I can:
1. Update RegisterViewModel (add CompanyName)
2. Update Register view (add company field)
3. Update Register controller (create tenant + user)
4. Create database migration (add trial fields)
5. Test full signup flow

**Estimated total time:** 2-3 hours

---

## 📋 RECOMMENDATION SUMMARY

Based on your system architecture (multi-tenant SaaS with trials), I recommend:

✅ **Option 1: Tenant + Owner**
✅ **Q2:** Yes - 14-day trial automatic
✅ **Q3:** Auto-downgrade to Free (keeps data)
✅ **Q4:** Add Company Name (required), rest optional
✅ **Q5:** Owner role (full admin)
✅ **Q6:** Partial - Steps 1-3 mandatory, rest optional
✅ **Q7:** Keep as is (dev: auto, prod: require)

**Why:** This matches how Slack, Notion, Asana, and other B2B SaaS work.

---

**Status:** ⚠️ AWAITING YOUR DECISION
**Do NOT code until you confirm which option + answer Q1-Q7**
**Date:** 2026-01-13
