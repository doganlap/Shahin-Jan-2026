# 3 BUTTONS ON ONBOARDING PAGE - EXPLANATION

## The Problem
You said: "all visible and all access the same" - this means all buttons seem to go to the same place (dashboard). This happens because:

**If you're already logged in AND your onboarding is completed:**
- Button 1 (Wizard) → Redirects to Dashboard (wizard already done)
- Button 2 (OrgSetup) → Goes to OrgSetup page
- Button 3 (New Org) → Goes to Signup page

**If you're NOT logged in:**
- Button 1 (Wizard) → Redirects to Login page
- Button 2 (OrgSetup) → Not visible (only shows when logged in)
- Button 3 (New Org) → Goes to Signup page (should work now)

---

## BUTTON 1: "Start 12-Step Wizard" (🟢 GREEN)

**What it does:**
- Starts the guided 12-step onboarding questionnaire (Steps A-L)
- Asks 96 questions about your organization

**Goes to:** `/OnboardingWizard/StepA/{tenantId}`

**When to use:**
- ✅ First time setting up your organization
- ✅ Want guided step-by-step configuration
- ✅ Need compliance scope automatically derived

**What happens:**
1. If wizard NOT completed → Shows Step A (Organization Identity)
2. If wizard ALREADY completed → Redirects to Dashboard with message

**Result:**
- Creates OrganizationProfile
- Runs Rules Engine → Derives baselines, packages, templates
- Creates Plan with phases
- Sets up teams and workflows

---

## BUTTON 2: "Organization Setup" (🔵 BLUE)

**What it does:**
- Opens manual organization configuration dashboard
- Alternative to wizard - you configure things manually

**Goes to:** `/OrgSetup`

**When to use:**
- ✅ Want to skip the wizard and configure manually
- ✅ Need to edit organization settings after setup
- ✅ Want to manage teams/users directly

**Visibility:**
- ✅ ONLY shows when you're logged in
- ❌ Hidden if not authenticated

**What happens:**
- Shows dashboard with:
  - Organization summary
  - Teams management
  - Users management
  - Setup progress

**Result:**
- Manual configuration (no automatic derivation)
- You set up everything yourself

---

## BUTTON 3: "New Organization" (⚪ GRAY)

**What it does:**
- Opens organization registration/signup form
- Creates a NEW tenant/organization (different from your current one)

**Goes to:** `/Onboarding/Signup`

**When to use:**
- ✅ Want to register a DIFFERENT organization
- ✅ Need multiple organizations/tenants
- ✅ Starting a new trial for another company

**Visibility:**
- ✅ Always visible (public access)

**What happens:**
- Shows form to create new organization:
  - Organization Name
  - Admin Email
  - Tenant Slug
- Creates new tenant in database

**Result:**
- New organization/tenant created
- You'll need to complete onboarding for this NEW organization separately

---

## SUMMARY TABLE

| Button | Color | When Visible | Where It Goes | When to Use |
|--------|-------|--------------|---------------|-------------|
| **Start 12-Step Wizard** | 🟢 Green | Always | Wizard Step A (or Dashboard if completed) | First-time setup, guided |
| **Organization Setup** | 🔵 Blue | Only when logged in | OrgSetup dashboard | Manual setup, editing |
| **New Organization** | ⚪ Gray | Always | Signup form | Create NEW organization |

---

## WHY ALL BUTTONS SEEM THE SAME

If you're clicking and they all go to Dashboard, it's because:

1. **You're logged in**
2. **Your wizard is already completed** (status = "Completed")
3. **Button 1 redirects completed wizards to Dashboard**

**Solution:**
- To see the wizard again, you'd need to either:
  - Reset the wizard status in database, OR
  - Create a new organization (Button 3)

---

## FIXES APPLIED

1. ✅ Added `[AllowAnonymous]` to Signup action (Button 3 should work without login now)
2. ✅ Added check in Wizard to redirect completed wizards to Dashboard
3. ✅ Added logging to track button clicks
4. ✅ Fixed button link to use `Url.Action()` for correct routing

---

## NEXT STEPS TO TEST

1. **Test Button 3 (New Organization)** - Should work without login now
2. **Test Button 1 (Wizard)** - If wizard completed, will redirect to Dashboard (expected)
3. **Test Button 2 (OrgSetup)** - Only visible when logged in
