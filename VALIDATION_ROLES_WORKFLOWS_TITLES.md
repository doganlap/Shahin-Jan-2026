# ✅ VALIDATION REPORT: ROLES, WORKFLOWS & TITLES
## What Actually Exists in the Code

---

## 📊 SUMMARY

| Component | Status | Count | Location |
|-----------|--------|-------|----------|
| **RoleProfiles** (Legacy) | ✅ Exists | 15 | `RoleProfileSeeds.cs` |
| **RoleCatalog** (Current) | ✅ Exists | 14+ | `CatalogSeederService.cs` |
| **WorkflowDefinitions** | ✅ Exists | 7 | `WorkflowDefinitionSeeds.cs` |
| **TitleCatalog** | ✅ Exists | Auto-generated | Per role (Jr/Standard/Sr) |
| **RoleProfile UI** | ✅ Exists | 1 view | `/RoleProfile/Index` |
| **Title Management UI** | ❌ Missing | 0 views | Needs creation |

---

## 🔒 PREDEFINED ROLES

### Source 1: RoleProfiles (15 Roles) - `RoleProfileSeeds.cs`

| Layer | Code | Name | Approval Level | Workflows |
|-------|------|------|----------------|-----------|
| **Executive** | `CRO` | Chief Risk Officer | 4 (Org-wide) | NCA, SAMA, ERM, Finding |
| **Executive** | `CCO` | Chief Compliance Officer | 4 (Org-wide) | SAMA, PDPL, Policy, Evidence |
| **Executive** | `ED` | Executive Director | 4 (Org-wide) | Policy |
| **Management** | `RM` | Risk Manager | 3 (Dept) | NCA, ERM, Finding |
| **Management** | `CM` | Compliance Manager | 3 (Dept) | SAMA, Evidence |
| **Management** | `AM` | Audit Manager | 3 (Dept) | Finding, Evidence |
| **Management** | `SM` | Security Manager | 3 (Dept) | SAMA, PDPL |
| **Management** | `LM` | Legal Manager | 3 (Dept) | PDPL, Policy |
| **Operational** | `CO` | Compliance Officer | 2 (Team) | SAMA, Evidence |
| **Operational** | `RA` | Risk Analyst | 1 (Own) | NCA, ERM |
| **Operational** | `PO` | Privacy Officer | 2 (Team) | PDPL |
| **Operational** | `QA` | Quality Assurance Mgr | 2 (Team) | Finding |
| **Operational** | `ProcOwner` | Process Owner | 1 (Own) | Finding |
| **Support** | `DS` | Documentation Specialist | 0 | Policy |
| **Support** | `RA_Report` | Reporting Analyst | 0 | NCA, SAMA, ERM |

**File:** `src/GrcMvc/Data/Seeds/RoleProfileSeeds.cs`
**Table:** `RoleProfiles`
**Seeded:** ✅ Yes (in `ApplicationInitializer.InitializeAsync()`)

---

### Source 2: RoleCatalog (14+ Roles) - `CatalogSeederService.cs`

| Layer | Code | Name | Approval Level | Can Approve |
|-------|------|------|----------------|-------------|
| **Executive** | `CEO` | Chief Executive Officer | 4 | ✅ |
| **Executive** | `CRO` | Chief Risk Officer | 4 | ✅ |
| **Executive** | `CISO` | Chief Information Security Officer | 4 | ✅ |
| **Executive** | `CCO` | Chief Compliance Officer | 4 | ✅ |
| **Executive** | `DPO` | Data Protection Officer | 3 | ✅ |
| **Management** | `COMPLIANCE_MANAGER` | Compliance Manager | 3 | ✅ |
| **Management** | `RISK_MANAGER` | Risk Manager | 3 | ✅ |
| **Management** | `AUDIT_MANAGER` | Audit Manager | 3 | ✅ |
| **Management** | `LEGAL_COUNSEL` | Legal Counsel | 3 | ✅ |
| **Operational** | `CONTROL_OWNER` | Control Owner | 2 | ❌ |
| **Operational** | `RISK_ANALYST` | Risk Analyst | 1 | ❌ |
| **Operational** | `PRIVACY_ANALYST` | Privacy Analyst | 1 | ❌ |
| **Operational** | `AUDITOR` | Auditor | 2 | ❌ |
| **Operational** | `POLICY_OWNER` | Policy Owner | 2 | ❌ |
| **Operational** | `ACTION_OWNER` | Action Owner | 1 | ❌ |
| **Operational** | `OPERATIONS_MANAGER` | Operations Manager | 2 | ❌ |
| **Support** | `SME` | Subject Matter Expert | 1 | ❌ |
| **Support** | `POLICY_ADMIN` | Policy Administrator | 1 | ❌ |
| **Support** | `PROCESS_OWNER` | Process Owner | 2 | ❌ |

**File:** `src/GrcMvc/Services/Implementations/CatalogSeederService.cs`
**Table:** `RoleCatalogs`
**Seeded:** ⚠️ Conditionally (via `SeedRolesAndTitlesAsync()`)

---

## ⚠️ DUPLICATE ROLE SYSTEMS

**Issue Found:** There are TWO separate role systems:

1. **RoleProfiles** (`RoleProfileSeeds.cs`) → Table: `RoleProfiles`
2. **RoleCatalog** (`CatalogSeederService.cs`) → Table: `RoleCatalogs`

**They have different fields:**

| Field | RoleProfile | RoleCatalog |
|-------|-------------|-------------|
| RoleCode | ✅ | ✅ |
| RoleName | ✅ | ✅ |
| Layer | ✅ | ✅ |
| Department | ✅ | ✅ |
| ApprovalLevel | ✅ | ✅ |
| ApprovalAuthority | ✅ (decimal) | ❌ |
| CanApprove | ✅ | ✅ |
| CanReject | ✅ | ✅ |
| CanEscalate | ✅ | ✅ |
| CanReassign | ✅ | ✅ |
| ParticipatingWorkflows | ✅ | ❌ |
| Scope | ✅ | ❌ |
| Responsibilities | ✅ (JSON) | ❌ |
| AllowedTitles | ❌ | ✅ (Navigation) |

**Recommendation:** Consolidate to use only `RoleCatalog` with extended fields.

---

## 🔄 PREDEFINED WORKFLOWS (7)

| # | Workflow Number | Name | Steps | Framework |
|---|-----------------|------|-------|-----------|
| 1 | `WF-NCA-ECC-001` | NCA ECC Assessment | 8 | NCA |
| 2 | `WF-SAMA-CSF-001` | SAMA CSF Assessment | 7 | SAMA |
| 3 | `WF-PDPL-PIA-001` | PDPL Privacy Impact Assessment | 7 | PDPL |
| 4 | `WF-ERM-001` | Enterprise Risk Management | 7 | ERM |
| 5 | `WF-EVIDENCE-001` | Evidence Review & Approval | 6 | Evidence |
| 6 | `WF-FINDING-REMEDIATION-001` | Audit Finding Remediation | 8 | Finding |
| 7 | `WF-POLICY-001` | Policy Review & Publication | 7 | Policy |

**File:** `src/GrcMvc/Data/Seeds/WorkflowDefinitionSeeds.cs`
**Table:** `WorkflowDefinitions`
**Seeded:** ✅ Yes (in `ApplicationInitializer.InitializeAsync()`)

---

## ✅ TITLE CATALOG

### Auto-Generated Titles per Role

The `CatalogSeederService.cs` auto-generates 3 titles per role:

```csharp
foreach (var role in roles)
{
    titles.Add(new TitleCatalog
    {
        TitleCode = $"JR_{role.RoleCode}",
        TitleName = $"Junior {role.RoleName}",
        ...
    });
    titles.Add(new TitleCatalog
    {
        TitleCode = role.RoleCode,
        TitleName = role.RoleName,
        ...
    });
    titles.Add(new TitleCatalog
    {
        TitleCode = $"SR_{role.RoleCode}",
        TitleName = $"Senior {role.RoleName}",
        ...
    });
}
```

**Result:** For each role, 3 titles are created:
- `JR_COMPLIANCE_MANAGER` → "Junior Compliance Manager"
- `COMPLIANCE_MANAGER` → "Compliance Manager"
- `SR_COMPLIANCE_MANAGER` → "Senior Compliance Manager"

**Total Titles:** ~57 (19 roles × 3 titles)

**File:** `src/GrcMvc/Services/Implementations/CatalogSeederService.cs`
**Table:** `TitleCatalogs`

---

## 🖥️ UI COMPONENTS

### What Exists ✅

| Route | Controller | View | Purpose |
|-------|------------|------|---------|
| `/RoleProfile` | `RoleProfileController` | `Index.cshtml` | Dashboard with roles & titles |
| `/RoleProfile/Roles` | `RoleProfileController` | ❌ Missing | View all roles |
| `/RoleProfile/Titles` | `RoleProfileController` | ❌ Missing | View all titles |
| `/RoleProfile/MyProfile` | `RoleProfileController` | ❌ Missing | User's own profile |

### What's Missing ❌

| Route | Purpose | Priority |
|-------|---------|----------|
| `/RoleProfile/Roles` view | Display all predefined roles | ⚡ HIGH |
| `/RoleProfile/Titles` view | Display all titles | ⚡ HIGH |
| `/OrgSetup/Titles` | Org-specific title management | 🔥 CRITICAL |
| Title CRUD operations | Create/Edit/Delete tenant titles | 🔥 CRITICAL |

---

## 🗄️ DATABASE TABLES

### Verified Tables

```sql
-- Roles (Two systems)
SELECT * FROM RoleProfiles;      -- 15 records (RoleProfileSeeds)
SELECT * FROM RoleCatalogs;      -- 14+ records (CatalogSeederService)

-- Workflows
SELECT * FROM WorkflowDefinitions; -- 7 records

-- Titles
SELECT * FROM TitleCatalogs;       -- ~57 records (auto-generated)

-- User Assignment
SELECT * FROM TenantUsers;         -- Links User → Tenant → RoleCode → TitleCode
```

### Schema Verification

```csharp
// GrcDbContext.cs confirms:
public DbSet<RoleProfile> RoleProfiles { get; set; }        // Line 155
public DbSet<RoleCatalog> RoleCatalogs { get; set; }        // Line 189
public DbSet<TitleCatalog> TitleCatalogs { get; set; }      // Line 190
public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; } // Line 141
public DbSet<TenantUser> TenantUsers { get; set; }          // Line 68
```

---

## 🔗 HOW ROLES CONNECT TO USERS

### TenantUser Model

```csharp
// TenantUser.cs
public class TenantUser : BaseEntity
{
    public Guid TenantId { get; set; }
    public string UserId { get; set; }           // FK to AspNetUsers
    public string RoleCode { get; set; }         // e.g., "COMPLIANCE_OFFICER"
    public string TitleCode { get; set; }        // e.g., "SR_COMPLIANCE_OFFICER"
    public string Status { get; set; }           // Pending, Active, Suspended
    public string InvitationToken { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    ...
}
```

### Assignment in Code

```csharp
// TrialController.cs - Line 152-163
var tenantUser = new TenantUser
{
    Id = Guid.NewGuid(),
    TenantId = tenantId,
    UserId = user.Id,
    RoleCode = "TENANT_ADMIN",    // Hardcoded role
    TitleCode = "ADMIN",           // Hardcoded title
    Status = "Active",
    ...
};
```

---

## ⚠️ ISSUES FOUND

### Issue 1: Duplicate Role Systems
- `RoleProfiles` (15 roles) and `RoleCatalogs` (14+ roles) both exist
- Different fields, different purposes
- **Recommendation:** Use only `RoleCatalog`, migrate `RoleProfiles` features

### Issue 2: Missing Views
- `/RoleProfile/Roles` action exists but no view
- `/RoleProfile/Titles` action exists but no view
- **Recommendation:** Create missing views

### Issue 3: Title Management Not Tenant-Specific
- `TitleCatalog` has `RoleCatalogId` but no `TenantId`
- Titles are global, not per-organization
- **Recommendation:** Add `TenantId` to `TitleCatalog` for custom org titles

### Issue 4: Hardcoded Role/Title on Registration
- Trial registration uses hardcoded "TENANT_ADMIN" role
- Should use catalog lookup
- **Recommendation:** Lookup from `RoleCatalog`

---

## 📋 VALIDATION CHECKLIST

### ✅ What Works

- [x] 15 RoleProfiles seeded correctly
- [x] 7 WorkflowDefinitions seeded correctly
- [x] RoleCatalog with 14+ roles exists
- [x] TitleCatalog auto-generates 3 titles per role
- [x] TenantUser links users to roles/titles
- [x] RoleProfile Index view displays data
- [x] Workflows have BPMN XML and step definitions

### ❌ What Needs Work

- [ ] Consolidate RoleProfiles and RoleCatalog
- [ ] Create `/RoleProfile/Roles` view
- [ ] Create `/RoleProfile/Titles` view
- [ ] Add TenantId to TitleCatalog for org-specific titles
- [ ] Create `/OrgSetup/Titles` for tenant title management
- [ ] Add Title CRUD operations
- [ ] Add Arabic names for titles

---

## 🎯 RECOMMENDED ACTIONS

### Priority 1: Consolidate Roles
1. Keep `RoleCatalog` as the single source
2. Add missing fields from `RoleProfile` (ParticipatingWorkflows, Responsibilities)
3. Deprecate `RoleProfiles` table

### Priority 2: Fix Title System
1. Add `TenantId` to `TitleCatalog` (nullable for global, set for tenant-specific)
2. Create Title management UI at `/OrgSetup/Titles`
3. Add Arabic support (`TitleNameAr`)

### Priority 3: Create Missing Views
1. `/RoleProfile/Roles` - List all predefined roles
2. `/RoleProfile/Titles` - List all titles
3. `/RoleProfile/MyProfile` - User's own role/title info

---

## 📁 FILE LOCATIONS SUMMARY

| Purpose | File Path |
|---------|-----------|
| RoleProfile Seeds | `src/GrcMvc/Data/Seeds/RoleProfileSeeds.cs` |
| RoleCatalog Seeds | `src/GrcMvc/Services/Implementations/CatalogSeederService.cs` |
| Workflow Seeds | `src/GrcMvc/Data/Seeds/WorkflowDefinitionSeeds.cs` |
| RoleProfile Entity | `src/GrcMvc/Models/Entities/RoleProfile.cs` |
| RoleCatalog Entity | `src/GrcMvc/Models/Entities/Catalogs/CatalogEntities.cs` |
| TitleCatalog Entity | `src/GrcMvc/Models/Entities/Catalogs/CatalogEntities.cs` |
| WorkflowDefinition Entity | `src/GrcMvc/Models/Entities/WorkflowDefinition.cs` |
| TenantUser Entity | `src/GrcMvc/Models/Entities/TenantUser.cs` |
| RoleProfile Controller | `src/GrcMvc/Controllers/RoleProfileController.cs` |
| RoleProfile View | `src/GrcMvc/Views/RoleProfile/Index.cshtml` |
| Application Initializer | `src/GrcMvc/Data/ApplicationInitializer.cs` |
| DbContext | `src/GrcMvc/Data/GrcDbContext.cs` |
