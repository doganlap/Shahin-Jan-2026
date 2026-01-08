# 🗄️ DATABASE STRUCTURE VALIDATION
## Complete Analysis of GRC System Data Model

---

## 📊 SUMMARY STATISTICS

| Category | Count | Tables/Entities |
|----------|-------|-----------------|
| **Total DbSets** | 190 | All entities in GrcDbContext |
| **Multi-Tenant Core** | 6 | Tenant, TenantUser, OrganizationProfile, OnboardingWizard, etc. |
| **Workspace** | 5 | Workspace, WorkspaceMembership, WorkspaceControl, ApprovalGates |
| **Teams & RACI** | 3 | Team, TeamMember, RACIAssignment |
| **Catalogs (Layer 1)** | 8 | Regulator, Framework, Control, Role, Title, Baseline, Package, Template |
| **GRC Core** | 20+ | Risk, Control, Assessment, Audit, Evidence, Policy, etc. |
| **Workflow** | 15+ | WorkflowDefinition, WorkflowInstance, WorkflowTask, etc. |
| **Integrations** | 10+ | IntegrationConnector, SyncJob, ERP, CCM, etc. |

---

## 🏗️ LAYER 1: GLOBAL CATALOGS (Platform-Wide)

These are predefined and **NOT tenant-specific** - seeded once and shared.

### 1.1 RegulatorCatalog
| Field | Type | Description |
|-------|------|-------------|
| Code | string(20) | e.g., "NCA", "SAMA", "ISO" |
| NameAr | string(200) | Arabic name |
| NameEn | string(200) | English name |
| Category | string(50) | cybersecurity, financial, healthcare |
| Sector | string(50) | all, banking_finance, healthcare |
| RegionType | string(20) | saudi, international, regional |

**Seed Data:** 92 regulators from CSV (62 Saudi, 20 International, 10 GCC)

### 1.2 FrameworkCatalog
| Field | Type | Description |
|-------|------|-------------|
| Code | string(50) | e.g., "NCA-ECC", "SAMA-CSF" |
| Version | string(20) | e.g., "2.0", "1.1" |
| TitleEn/TitleAr | string(300) | Bilingual titles |
| RegulatorId | Guid? | FK to RegulatorCatalog |
| IsMandatory | bool | Required by law? |
| ControlCount | int | Number of controls |

**Seed Data:** 163+ frameworks from CSV

### 1.3 ControlCatalog
| Field | Type | Description |
|-------|------|-------------|
| ControlId | string(50) | e.g., "NCA-ECC-1-1-1" |
| FrameworkId | Guid | FK to FrameworkCatalog |
| ControlNumber | string(50) | e.g., "1.1.1", "AC-1" |
| Domain/Subdomain | string(100) | Control category |
| TitleAr/TitleEn | string(500) | Bilingual |
| RequirementAr/En | string(4000) | Full requirement text |
| MappingIso27001 | string(100) | Cross-framework mapping |
| MappingNistCsf | string(100) | Cross-framework mapping |

**Seed Data:** 13,500+ controls from CSV (8.5MB file)

### 1.4 RoleCatalog
| Field | Type | Description |
|-------|------|-------------|
| RoleCode | string(50) | e.g., "COMPLIANCE_OFFICER" |
| RoleName | string(100) | Display name |
| Layer | string(50) | Executive, Management, Operational, Support |
| Department | string(50) | Compliance, Risk, IT, etc. |
| ApprovalLevel | int | 0-4 hierarchy level |
| CanApprove/Reject/Escalate | bool | Workflow permissions |

**Seed Data:** 14+ roles via CatalogSeederService

### 1.5 TitleCatalog
| Field | Type | Description |
|-------|------|-------------|
| TitleCode | string(50) | e.g., "SR_COMPLIANCE_ANALYST" |
| TitleName | string(100) | Display name |
| RoleCatalogId | Guid | FK to parent role |
| Description | string(500) | Purpose |

**Seed Data:** Auto-generated 3 per role (Jr/Standard/Sr) = ~57 titles

**⚠️ ISSUE:** TitleCatalog has NO TenantId - cannot create org-specific titles!

### 1.6 BaselineCatalog
| Field | Type | Description |
|-------|------|-------------|
| BaselineCode | string(50) | e.g., "NCA_ECC_2024" |
| BaselineName | string(200) | Full name |
| RegulatorCode | string(50) | Source regulator |
| Version | string(100) | Version number |
| ControlCount | int | Controls in baseline |

### 1.7 PackageCatalog
Business-friendly groupings shown to users (not technical framework details).

### 1.8 TemplateCatalog
Assessment templates for different scenarios.

---

## 🏢 LAYER 2: TENANT CONTEXT (Per-Organization)

### 2.1 Tenant (Organization)

```csharp
public class Tenant : BaseEntity
{
    public string TenantSlug { get; set; }           // URL-friendly identifier
    public string OrganizationName { get; set; }    // Display name
    public string AdminEmail { get; set; }          // Admin email
    public string Status { get; set; }              // Pending, Active, Suspended
    
    // Subscription
    public string SubscriptionTier { get; set; }    // MVP, Professional, Enterprise
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    
    // Trial
    public bool IsTrial { get; set; }               // Is trial tenant?
    public DateTime? TrialStartsAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }      // Typically 7 days
    
    // Onboarding
    public string OnboardingStatus { get; set; }    // NOT_STARTED, IN_PROGRESS, COMPLETED
    public Guid? DefaultWorkspaceId { get; set; }   // Created during finalization
    public Guid? GrcPlanId { get; set; }            // Auto-generated plan
}
```

### 2.2 TenantUser (User Assignment)

```csharp
public class TenantUser : BaseEntity
{
    public Guid TenantId { get; set; }              // Which org
    public string UserId { get; set; }              // FK to AspNetUsers (Identity)
    public string RoleCode { get; set; }            // From RoleCatalog
    public string TitleCode { get; set; }           // From TitleCatalog
    public string Status { get; set; }              // Pending, Active, Suspended
    
    // Invitation
    public string InvitationToken { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
}
```

### 2.3 OrganizationProfile

Complete profile data from onboarding - 80+ fields covering:

| Section | Fields |
|---------|--------|
| Basic Info | OrganizationType, Sector, Country, EmployeeCount |
| Legal Entity | LegalEntityName, CR Number, Tax ID, Address |
| Financial | AnnualRevenueRange, IsPubliclyTraded, Auditor |
| Structure | ParentCompany, Subsidiaries, Branches |
| Contacts | CEO, CFO, CISO, DPO, Compliance Officer |
| Regulatory | Certifications, Licenses, Primary/Secondary Regulators |
| Data & Tech | DataTypes, HostingModel, CloudProviders |
| Third Parties | Vendors, VendorCount, CriticalVendors |
| Onboarding Meta | Status, Progress, CompletedAt |

### 2.4 OnboardingWizard

**The 96-question wizard entity with 12 sections (A-L):**

| Section | Questions | Key Fields |
|---------|-----------|------------|
| **A: Organization Identity** | 13 | OrganizationLegalNameEn/Ar, Country, OrgType, Sector |
| **B: Assurance Objective** | 5 | PrimaryDriver, TargetTimeline, DesiredMaturity |
| **C: Regulatory Applicability** | 7 | PrimaryRegulatorsJson, FrameworksJson, Certifications |
| **D: Scope Definition** | 9 | LegalEntities, BusinessUnits, Systems, Processes |
| **E: Data & Risk Profile** | 6 | DataTypes, PaymentCard, CrossBorder |
| **F: Technology Landscape** | 13 | IdentityProvider, SIEM, Cloud, ERP, CMDB |
| **G: Control Ownership** | 7 | OwnershipApproach, DefaultOwnerTeam, Approvers |
| **H: Teams & Access** | 10 | OrgAdmins, Teams, RACI, ApprovalGates |
| **I: Workflow & Cadence** | 10 | EvidenceFrequency, SLAs, Escalation |
| **J: Evidence Standards** | 7 | NamingConvention, Retention, AcceptableTypes |
| **K: Baseline & Overlays** | 3 | AdoptDefaultBaseline, SelectedOverlays |
| **L: Success Metrics** | 6 | SuccessMetrics, Baselines, PilotScope |

**Wizard Metadata:**
- `CurrentStep` (1-12)
- `WizardStatus` (NotStarted, InProgress, Completed)
- `ProgressPercent` (0-100)
- `CompletedSectionsJson`
- `AllAnswersJson` (raw audit trail)

---

## 🏪 LAYER 3: WORKSPACE (Market/BU Scope)

### 3.1 Workspace

```csharp
public class Workspace : BaseEntity
{
    public Guid TenantId { get; set; }              // Parent tenant
    public string WorkspaceCode { get; set; }       // "KSA", "UAE", "RETAIL"
    public string Name { get; set; }                // "Saudi Arabia Market"
    public string NameAr { get; set; }              // Arabic name
    public string WorkspaceType { get; set; }       // Market, BusinessUnit, Entity
    public string JurisdictionCode { get; set; }    // ISO country code
    public string DefaultLanguage { get; set; }     // "ar", "en"
    public bool IsDefault { get; set; }             // Default for new users?
    public string RegulatorsJson { get; set; }      // Workspace-specific regulators
    public string OverlaysJson { get; set; }        // Framework overlays
}
```

### 3.2 WorkspaceMembership

Links users to workspaces with roles.

### 3.3 WorkspaceControl

Which controls apply to which workspace (with overrides).

### 3.4 WorkspaceApprovalGate / Approvers

Approval workflows per workspace scope.

---

## 👥 LAYER 4: TEAMS & RACI

### 4.1 Team

```csharp
public class Team : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? WorkspaceId { get; set; }          // null = shared across all
    public string TeamCode { get; set; }            // "SEC-OPS", "IT-OPS"
    public string Name { get; set; }                // "Security Operations"
    public string TeamType { get; set; }            // Operational, Governance, Project
    public string BusinessUnit { get; set; }
    public Guid? ManagerUserId { get; set; }
    public bool IsDefaultFallback { get; set; }     // Fallback for unassigned work
    public bool IsSharedTeam { get; set; }          // Shared across workspaces
}
```

### 4.2 TeamMember

```csharp
public class TeamMember : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public string RoleCode { get; set; }            // Role within team
    public bool IsPrimaryForRole { get; set; }      // Primary contact for role
    public bool CanApprove { get; set; }            // Can approve for team
    public bool CanDelegate { get; set; }           // Can delegate tasks
}
```

### 4.3 RACIAssignment

```csharp
public class RACIAssignment : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? WorkspaceId { get; set; }          // null = global RACI
    public string ScopeType { get; set; }           // ControlFamily, System, Framework
    public string ScopeId { get; set; }             // "IAM", "Payments", "NCA-ECC"
    public Guid TeamId { get; set; }
    public string RACI { get; set; }                // R, A, C, or I
    public string RoleCode { get; set; }            // Specific role (optional)
}
```

---

## 🔒 ROLES SYSTEM (Dual Systems - Issue!)

### System 1: RoleProfiles (15 roles)
**File:** `RoleProfileSeeds.cs`
**Purpose:** Workflow-oriented roles with BPMN workflow participation

| Layer | Roles |
|-------|-------|
| Executive | CRO, CCO, ED |
| Management | RM, CM, AM, SM, LM |
| Operational | CO, RA, PO, QA, ProcOwner |
| Support | DS, RA_Report |

**Fields:** RoleCode, RoleName, Layer, ApprovalLevel, ApprovalAuthority, ParticipatingWorkflows, Responsibilities (JSON)

### System 2: RoleCatalogs (14+ roles)
**File:** `CatalogSeederService.cs`
**Purpose:** Catalog-style roles with Title navigation

| Layer | Roles |
|-------|-------|
| Executive | CEO, CRO, CISO, CCO, DPO |
| Management | COMPLIANCE_MANAGER, RISK_MANAGER, AUDIT_MANAGER, LEGAL_COUNSEL |
| Operational | CONTROL_OWNER, RISK_ANALYST, PRIVACY_ANALYST, AUDITOR, POLICY_OWNER, ACTION_OWNER, OPERATIONS_MANAGER |
| Support | SME, POLICY_ADMIN, PROCESS_OWNER |

**Fields:** RoleCode, RoleName, Layer, ApprovalLevel, CanApprove/Reject/Escalate, AllowedTitles (nav)

### ⚠️ CONSOLIDATION NEEDED

Both systems exist but serve different purposes. Recommendation:
1. Keep `RoleCatalog` as the single source
2. Migrate `ParticipatingWorkflows` and `Responsibilities` fields from RoleProfiles
3. Deprecate RoleProfiles table

---

## 🔄 WORKFLOW SYSTEM

### WorkflowDefinitions (7 predefined)

| # | WorkflowNumber | Name | Steps | Framework |
|---|----------------|------|-------|-----------|
| 1 | WF-NCA-ECC-001 | NCA ECC Assessment | 8 | NCA |
| 2 | WF-SAMA-CSF-001 | SAMA CSF Assessment | 7 | SAMA |
| 3 | WF-PDPL-PIA-001 | PDPL Privacy Impact Assessment | 7 | PDPL |
| 4 | WF-ERM-001 | Enterprise Risk Management | 7 | ERM |
| 5 | WF-EVIDENCE-001 | Evidence Review & Approval | 6 | Evidence |
| 6 | WF-FINDING-REMEDIATION-001 | Audit Finding Remediation | 8 | Finding |
| 7 | WF-POLICY-001 | Policy Review & Publication | 7 | Policy |

**Structure:**
```csharp
public class WorkflowDefinition : BaseEntity
{
    public Guid? TenantId { get; set; }             // null = global template
    public string WorkflowNumber { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }            // Assessment, Evidence, Policy
    public string Framework { get; set; }           // NCA, SAMA, PDPL, ERM
    public string Steps { get; set; }               // JSON array of step definitions
    public string BpmnXml { get; set; }             // BPMN diagram
}
```

### WorkflowInstance
Running instance of a workflow for a specific tenant/workspace.

### WorkflowTask
Individual tasks within a workflow instance with assignees.

---

## 🗂️ GRC CORE ENTITIES

| Entity | Purpose | Key Fields |
|--------|---------|------------|
| **Risk** | Risk register | Title, Category, Impact, Likelihood, Score |
| **Control** | Control register | ControlId, Title, Type, Status, Owner |
| **Assessment** | Compliance assessments | Name, Framework, Status, Score |
| **Audit** | Audit activities | Name, Type, Status, Findings |
| **AuditFinding** | Individual findings | Severity, Status, Remediation |
| **Evidence** | Evidence files | Title, Type, Status, FilePath |
| **Policy** | Policy documents | Title, Version, Status, ApprovedBy |
| **ActionPlan** | Remediation plans | Title, Priority, DueDate, Owner |
| **Vendor** | Third-party vendors | Name, RiskLevel, Contract |
| **ComplianceEvent** | Compliance calendar | EventType, DueDate, Recurrence |

---

## 📁 CSV SEED DATA

| File | Records | Size |
|------|---------|------|
| regulators_catalog_seed.csv | 92 | 14KB |
| frameworks_catalog_seed.csv | 163+ | 19KB |
| controls_catalog_seed.csv | 13,500+ | 8.5MB |

---

## 🔑 KEY RELATIONSHIPS

```
ApplicationUser (Identity DB)
    └── TenantUser (many) ──→ Tenant
         ├── RoleCode ──→ RoleCatalog
         ├── TitleCode ──→ TitleCatalog
         └── TeamMember (many) ──→ Team ──→ Workspace

Tenant
    ├── OrganizationProfile (1:1)
    ├── OnboardingWizard (1:1)
    ├── Workspaces (many)
    │    ├── WorkspaceMemberships (many)
    │    ├── WorkspaceControls (many)
    │    ├── WorkspaceApprovalGates (many)
    │    └── Teams (many)
    │         ├── TeamMembers (many)
    │         └── RACIAssignments (many)
    ├── Plans (many)
    ├── Risks (many)
    ├── Controls (many)
    ├── Assessments (many)
    └── ... (all GRC entities)
```

---

## ⚠️ ISSUES FOUND

### Issue 1: Duplicate Role Systems
- `RoleProfiles` (15) and `RoleCatalogs` (14+) both exist
- Different fields, different purposes
- **Fix:** Consolidate to RoleCatalog only

### Issue 2: TitleCatalog Not Tenant-Specific
- `TitleCatalog` has no `TenantId` field
- Cannot create organization-specific job titles
- **Fix:** Add nullable TenantId (null = global, set = tenant-specific)

### Issue 3: Large Control Catalog
- 13,500+ controls in single CSV
- Could cause slow initial seeding
- **Consider:** Lazy loading or pagination

### Issue 4: Onboarding Entity Has 96 Fields
- Very wide entity (96+ questions)
- All fields stored even if empty
- **Consider:** Progressive storage with ProfileQuestions pattern

### Issue 5: JSON Storage Pattern
- Many fields use JSON strings for arrays/objects
- Query performance limitations
- **OK for now:** Standard pattern for document-style storage in SQL

---

## ✅ WHAT'S WORKING WELL

1. ✅ **Multi-tenant architecture** - TenantId on all entities
2. ✅ **Workspace scoping** - Market/BU level granularity
3. ✅ **RACI assignments** - Proper responsibility mapping
4. ✅ **Bilingual support** - Ar/En fields throughout
5. ✅ **Workflow definitions** - 7 predefined workflows ready
6. ✅ **Catalog seeding** - 92 regulators, 163 frameworks, 13,500 controls
7. ✅ **Team structure** - Proper team/member/role relationships
8. ✅ **Onboarding wizard** - Comprehensive 96-question coverage

---

## 📋 NEXT STEPS RECOMMENDATIONS

1. **Consolidate Role Systems** → Single RoleCatalog
2. **Add TenantId to TitleCatalog** → Org-specific titles
3. **Create Title Management UI** → `/OrgSetup/Titles`
4. **Implement Progressive Onboarding** → Store questions progressively
5. **Add Missing Views** → Roles list, Titles list
