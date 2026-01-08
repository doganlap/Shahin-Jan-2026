# 👥 TEAM STRUCTURE & INVITATION FLOW
## From Trial Registration to Full Team Setup

---

## 📊 CURRENT SYSTEM OVERVIEW

### What Exists ✅

| Component | Table | Status |
|-----------|-------|--------|
| **Tenant** | `Tenants` | ✅ Created on trial registration |
| **Tenant Admin** | `TenantUsers` | ✅ Created on trial registration |
| **User Account** | `AspNetUsers` | ✅ Created with Identity |
| **Teams** | `Teams` | ✅ Table exists |
| **Team Members** | `TeamMembers` | ✅ Table exists |
| **Invitations** | `TenantUsers.InvitationToken` | ✅ Field exists |
| **RACI Assignments** | `RACIAssignments` | ✅ Table exists |

### User Journey ✅

```
Trial Registration → TenantAdmin created → Can invite team members
```

---

## 🏗️ TEAM STRUCTURE OPTIONS

### Option 1: Single Team (Simple - Recommended for Small Orgs)

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SINGLE TEAM MODE                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   TENANT: "Acme Corporation"                                        │
│   └── TEAM: "Shahin AI Team" (Default)                             │
│       ├── 👑 Ahmed (Tenant Admin) - Owner, Full Access             │
│       ├── 👤 Sara (Compliance Officer) - Assessments               │
│       ├── 👤 Mohammed (IT Security) - Evidence, Controls           │
│       └── 👤 Fatima (Auditor) - Reports, Read-Only                 │
│                                                                     │
│   Everyone works together, sees same dashboard.                     │
│   Simple workflow: Admin assigns tasks to team members.             │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

**Best For:**
- Small organizations (< 20 employees)
- Single location
- Trial users exploring the platform
- Simple compliance needs

---

### Option 2: Multiple Teams (Advanced - For Larger Orgs)

```
┌─────────────────────────────────────────────────────────────────────┐
│                        MULTIPLE TEAM MODE                           │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   TENANT: "Saudi National Bank"                                     │
│   │                                                                 │
│   ├── WORKSPACE: "KSA Operations"                                   │
│   │   ├── TEAM: "IT Security Team"                                 │
│   │   │   ├── 👤 Khalid (Security Lead) - Control Owner            │
│   │   │   ├── 👤 Nora (Security Analyst) - Evidence                │
│   │   │   └── 👤 Omar (Vulnerability Mgr) - Risk                   │
│   │   │                                                            │
│   │   ├── TEAM: "Compliance Team"                                  │
│   │   │   ├── 👤 Layla (Compliance Manager) - Assessments          │
│   │   │   ├── 👤 Tariq (Policy Officer) - Policies                 │
│   │   │   └── 👤 Huda (Audit Liaison) - Audits                     │
│   │   │                                                            │
│   │   └── TEAM: "Risk Management Team"                             │
│   │       ├── 👤 Salman (Risk Manager) - Risk Register             │
│   │       └── 👤 Dana (Business Analyst) - Reports                 │
│   │                                                                 │
│   └── WORKSPACE: "UAE Operations"                                   │
│       └── TEAM: "UAE Compliance Team"                              │
│           └── 👤 Rashid (Regional Lead) - All UAE                  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

**Best For:**
- Large organizations (50+ employees)
- Multiple locations/markets
- Complex compliance requirements
- Clear departmental separation

---

## 👤 USER ROLES IN THE SYSTEM

### Tenant-Level Roles (TenantUser.RoleCode)

| Role | Code | Permissions |
|------|------|-------------|
| **Tenant Admin** | `TENANT_ADMIN` | Full access, invite users, manage settings |
| **Compliance Officer** | `COMPLIANCE_OFFICER` | Assessments, policies, evidence |
| **Security Lead** | `SECURITY_LEAD` | Controls, vulnerabilities, incidents |
| **Risk Manager** | `RISK_MANAGER` | Risk register, action plans |
| **Auditor** | `AUDITOR` | Audits, read-only reports |
| **Evidence Custodian** | `EVIDENCE_CUSTODIAN` | Upload/manage evidence only |
| **Viewer** | `VIEWER` | Read-only access |

### Team-Level Roles (TeamMember.RoleCode)

| Role | Code | Team Permissions |
|------|------|------------------|
| **Team Lead** | `TEAM_LEAD` | Manage team, assign tasks |
| **Control Owner** | `CONTROL_OWNER` | Own and manage controls |
| **Assessor** | `ASSESSOR` | Perform assessments |
| **Approver** | `APPROVER` | Approve workflows |
| **Contributor** | `CONTRIBUTOR` | Submit work, no approval |

---

## 📧 INVITATION FLOW

### Current Flow (What Exists)

```
┌─────────────────────────────────────────────────────────────────────┐
│                      CURRENT INVITATION FLOW                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   STEP 1: Admin goes to /TenantAdmin/InviteUser                    │
│   ┌────────────────────────────────────────────────┐               │
│   │ Invite User                                    │               │
│   │                                                │               │
│   │ Email: [____________]                          │               │
│   │ Role: [Compliance Officer ▼]                  │               │
│   │ Title: [Analyst ▼] (optional)                 │               │
│   │                                                │               │
│   │ [Send Invitation]                              │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   STEP 2: System creates TenantUser with:                          │
│   ├── Status = "Pending"                                           │
│   ├── InvitationToken = (generated)                                │
│   ├── InvitedAt = DateTime.UtcNow                                  │
│   └── InvitedBy = AdminUserId                                      │
│                          ↓                                         │
│   STEP 3: Email sent to invitee                                    │
│   ┌────────────────────────────────────────────────┐               │
│   │ Subject: You've been invited to join [Org]    │               │
│   │                                                │               │
│   │ Hi [Name],                                    │               │
│   │ You've been invited to join [Org] on Shahin   │               │
│   │ GRC Platform as a [Role].                     │               │
│   │                                                │               │
│   │ [Accept Invitation]                           │               │
│   │                                                │               │
│   │ This link expires in 7 days.                  │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   STEP 4: Invitee clicks link, goes to /Account/AcceptInvite      │
│   ┌────────────────────────────────────────────────┐               │
│   │ Welcome to [Org]!                             │               │
│   │                                                │               │
│   │ Create your password:                          │               │
│   │ Password: [____________]                       │               │
│   │ Confirm: [____________]                        │               │
│   │                                                │               │
│   │ [Complete Setup]                               │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   STEP 5: TenantUser updated:                                      │
│   ├── Status = "Active"                                            │
│   ├── ActivatedAt = DateTime.UtcNow                                │
│   └── User linked to AspNetUsers                                   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🆕 PROPOSED IMPROVED FLOW

### Progressive Team Building (Ask When Needed)

```
┌─────────────────────────────────────────────────────────────────────┐
│                    IMPROVED INVITATION FLOW                         │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   AFTER QUICK SETUP (Tier 1):                                       │
│   ┌────────────────────────────────────────────────┐               │
│   │ ✅ You're all set up!                          │               │
│   │                                                │               │
│   │ Next: Would you like to invite team members?  │               │
│   │                                                │               │
│   │ [👤 Yes, invite now] [Later →]                │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   IF "Yes, invite now":                                            │
│   ┌────────────────────────────────────────────────┐               │
│   │ How does your team work?                       │               │
│   │                                                │               │
│   │ ○ We all work together (1 team)               │               │
│   │   → Simple setup, everyone sees everything    │               │
│   │                                                │               │
│   │ ○ We have separate departments (multiple)     │               │
│   │   → Create teams: IT, Compliance, Risk, etc.  │               │
│   │                                                │               │
│   │ [Continue]                                     │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   SINGLE TEAM MODE:                                                │
│   ┌────────────────────────────────────────────────┐               │
│   │ Invite Team Members                            │               │
│   │                                                │               │
│   │ Email            | Role                        │               │
│   │ [___________]    | [Compliance Officer ▼]     │               │
│   │ [___________]    | [Security Lead ▼]          │               │
│   │ [___________]    | [Auditor ▼]                │               │
│   │ [+ Add another]                               │               │
│   │                                                │               │
│   │ [Send All Invitations]                        │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   MULTIPLE TEAM MODE:                                              │
│   ┌────────────────────────────────────────────────┐               │
│   │ Step 1: Create Teams                           │               │
│   │                                                │               │
│   │ ☑ IT Security Team                            │               │
│   │ ☑ Compliance Team                             │               │
│   │ ☑ Risk Management Team                        │               │
│   │ ☐ Internal Audit Team                         │               │
│   │ ☐ Legal Team                                  │               │
│   │ [+ Custom Team: ___________]                  │               │
│   │                                                │               │
│   │ [Continue]                                     │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   ┌────────────────────────────────────────────────┐               │
│   │ Step 2: Invite to Teams                        │               │
│   │                                                │               │
│   │ IT Security Team:                              │               │
│   │ Email            | Role                        │               │
│   │ [___________]    | [Team Lead ▼]              │               │
│   │ [___________]    | [Control Owner ▼]          │               │
│   │                                                │               │
│   │ Compliance Team:                               │               │
│   │ Email            | Role                        │               │
│   │ [___________]    | [Team Lead ▼]              │               │
│   │ [___________]    | [Assessor ▼]               │               │
│   │                                                │               │
│   │ [Send All Invitations]                        │               │
│   └────────────────────────────────────────────────┘               │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🗄️ DATABASE TABLES SUMMARY

### Existing Tables (What We Have)

```sql
-- 1. Tenants (Organization)
CREATE TABLE Tenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationName NVARCHAR(255),
    TenantSlug NVARCHAR(100),
    AdminEmail NVARCHAR(256),
    Status NVARCHAR(50), -- 'Active', 'Suspended'
    IsTrial BIT,
    TrialEndsAt DATETIME2,
    ...
);

-- 2. AspNetUsers (Identity)
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) PRIMARY KEY,
    Email NVARCHAR(256),
    UserName NVARCHAR(256),
    PasswordHash NVARCHAR(MAX),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    ...
);

-- 3. TenantUsers (Links User to Tenant with Role)
CREATE TABLE TenantUsers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER FK,
    UserId NVARCHAR(450) FK,        -- Links to AspNetUsers
    RoleCode NVARCHAR(100),          -- TENANT_ADMIN, COMPLIANCE_OFFICER, etc.
    TitleCode NVARCHAR(100),         -- Optional specialization
    Status NVARCHAR(50),             -- Pending, Active, Suspended
    InvitationToken NVARCHAR(256),   -- For email verification
    InvitedAt DATETIME2,
    InvitedBy NVARCHAR(450),
    ActivatedAt DATETIME2,
    ...
);

-- 4. Teams (Departments/Groups)
CREATE TABLE Teams (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER FK,
    WorkspaceId UNIQUEIDENTIFIER FK, -- Optional workspace scope
    TeamCode NVARCHAR(50),           -- TEAM-001, IT-SEC, COMPLIANCE
    Name NVARCHAR(255),
    NameAr NVARCHAR(255),
    Purpose NVARCHAR(500),
    TeamType NVARCHAR(50),           -- Operational, Governance, Project
    ManagerUserId UNIQUEIDENTIFIER,
    IsDefaultFallback BIT,
    IsSharedTeam BIT,
    IsActive BIT,
    ...
);

-- 5. TeamMembers (Links Users to Teams)
CREATE TABLE TeamMembers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER FK,
    WorkspaceId UNIQUEIDENTIFIER FK,
    TeamId UNIQUEIDENTIFIER FK,
    UserId UNIQUEIDENTIFIER FK,      -- Links to TenantUsers.Id
    RoleCode NVARCHAR(100),          -- TEAM_LEAD, CONTROL_OWNER, ASSESSOR
    IsPrimaryForRole BIT,
    CanApprove BIT,
    CanDelegate BIT,
    JoinedDate DATETIME2,
    IsActive BIT,
    ...
);

-- 6. RACIAssignments (Who does what)
CREATE TABLE RACIAssignments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER FK,
    WorkspaceId UNIQUEIDENTIFIER FK,
    ScopeType NVARCHAR(50),          -- ControlFamily, System, Framework
    ScopeId NVARCHAR(255),           -- IAM, NCA-ECC, Payments
    TeamId UNIQUEIDENTIFIER FK,
    RACI NCHAR(1),                   -- R, A, C, I
    RoleCode NVARCHAR(100),          -- Optional: specific role within team
    Priority INT,
    IsActive BIT,
    ...
);
```

---

## 🎯 TRIAL USER JOURNEY - COMPLETE FLOW

```
┌─────────────────────────────────────────────────────────────────────┐
│                   COMPLETE TRIAL USER JOURNEY                       │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   DAY 0: REGISTRATION                                               │
│   ├── User visits /trial                                           │
│   ├── Fills: Org Name, Full Name, Email, Password                  │
│   ├── Creates: Tenant + AspNetUser + TenantUser (TENANT_ADMIN)     │
│   ├── Auto-login                                                   │
│   └── Redirect to Quick Setup                                      │
│                                                                     │
│   DAY 0: QUICK SETUP (5 Questions)                                  │
│   ├── Country, Sector, Org Type, etc.                              │
│   ├── Auto-derive: Baselines, Frameworks                           │
│   └── Redirect to Dashboard                                        │
│                                                                     │
│   DAY 0-1: DASHBOARD (First Use)                                    │
│   ├── See: Welcome, First Steps widget                             │
│   ├── Option: "Invite team members" prompt                         │
│   └── Option: Start first assessment                               │
│                                                                     │
│   DAY 1: INVITE TEAM (When Ready)                                   │
│   ├── Choose: Single team or Multiple teams                        │
│   ├── Enter emails + roles                                         │
│   ├── Creates: TenantUser records (Status=Pending)                 │
│   └── Sends: Invitation emails                                     │
│                                                                     │
│   DAY 1-3: TEAM JOINS                                               │
│   ├── Team members receive email                                   │
│   ├── Click link → Create password                                 │
│   ├── TenantUser status → Active                                   │
│   └── Can now access platform with assigned role                   │
│                                                                     │
│   DAY 1-7: TEAM WORKS TOGETHER                                      │
│   ├── Admin assigns tasks to team members                          │
│   ├── Team members upload evidence, complete assessments           │
│   ├── Workflows route to right approvers                           │
│   └── Everyone sees progress on dashboard                          │
│                                                                     │
│   DAY 7: TRIAL ENDS                                                 │
│   ├── Countdown warning visible                                    │
│   ├── Upgrade prompt shown                                         │
│   └── Data preserved if upgraded, archived if not                  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📋 WHAT NEEDS TO BE ADDED

### Current Gaps

| Feature | Status | Priority |
|---------|--------|----------|
| Single vs Multiple team choice | ❌ Missing | 🔥 HIGH |
| Bulk invite (multiple at once) | ❌ Missing | ⚡ MEDIUM |
| Team creation during onboarding | ⚠️ Exists in wizard only | ⚡ MEDIUM |
| Invitation email templates | ⚠️ Basic exists | ⚠️ LOW |
| Auto-create default team | ❌ Missing | 🔥 HIGH |
| Team member dashboard | ⚠️ Partial | ⚠️ MEDIUM |

### Proposed New Components

1. **Auto-Create Default Team on Registration**
```csharp
// In TrialController.Register, after tenant creation:
var defaultTeam = new Team
{
    TenantId = tenant.Id,
    TeamCode = "TEAM-001",
    Name = $"{tenant.OrganizationName} Team",
    NameAr = "فريق العمل",
    Purpose = "Default team for all members",
    TeamType = "Operational",
    IsDefaultFallback = true,
    IsActive = true
};
_context.Teams.Add(defaultTeam);

// Add admin as first team member
var teamMember = new TeamMember
{
    TenantId = tenant.Id,
    TeamId = defaultTeam.Id,
    UserId = tenantUser.Id,
    RoleCode = "TEAM_LEAD",
    IsPrimaryForRole = true,
    CanApprove = true,
    CanDelegate = true,
    IsActive = true
};
_context.TeamMembers.Add(teamMember);
```

2. **Team Choice in Onboarding**
```html
<!-- After Quick Setup, before Dashboard -->
<div class="card">
    <div class="card-header">How does your team work?</div>
    <div class="card-body">
        <div class="form-check">
            <input type="radio" name="teamMode" value="single" id="singleTeam" checked>
            <label for="singleTeam">
                <strong>Single Team</strong>
                <small>Everyone works together in one team</small>
            </label>
        </div>
        <div class="form-check">
            <input type="radio" name="teamMode" value="multiple" id="multipleTeams">
            <label for="multipleTeams">
                <strong>Multiple Teams</strong>
                <small>Different departments/teams with separate workflows</small>
            </label>
        </div>
    </div>
</div>
```

3. **Bulk Invite Component**
```html
<div id="bulkInvite">
    <table class="table">
        <thead>
            <tr>
                <th>Email</th>
                <th>Role</th>
                <th>Team</th>
                <th></th>
            </tr>
        </thead>
        <tbody id="inviteRows">
            <tr>
                <td><input type="email" name="invites[0].email"></td>
                <td><select name="invites[0].role">...</select></td>
                <td><select name="invites[0].team">...</select></td>
                <td><button onclick="removeRow(this)">×</button></td>
            </tr>
        </tbody>
    </table>
    <button onclick="addRow()">+ Add Another</button>
    <button type="submit">Send All Invitations</button>
</div>
```

---

## ✅ IMPLEMENTATION CHECKLIST

### Phase 1: Core Infrastructure
- [ ] Auto-create default team on trial registration
- [ ] Add admin as team lead in default team
- [ ] Add "Invite Team" button to dashboard

### Phase 2: Invitation Flow
- [ ] Create bulk invite component
- [ ] Add team choice modal (single vs multiple)
- [ ] Improve invitation email template

### Phase 3: Team Management
- [ ] Team creation UI in onboarding
- [ ] Team member assignment UI
- [ ] RACI assignment UI

### Phase 4: Polish
- [ ] Team dashboard widgets
- [ ] Member activity tracking
- [ ] Role-based navigation

---

## 🎯 SUMMARY

**Current State:**
- ✅ Tenant Admin is created on trial registration
- ✅ Can invite team members manually
- ✅ Teams and TeamMembers tables exist
- ❌ No default team auto-created
- ❌ No team choice during onboarding
- ❌ No bulk invite

**Recommended Improvements:**
1. Auto-create default "Shahin AI Team" on registration
2. Add team choice: single vs multiple
3. Add bulk invite for multiple team members
4. Progressive: ask about teams when user tries to assign tasks

**Result:** Smooth journey from solo trial user → team collaboration
