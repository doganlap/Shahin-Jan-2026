# 🎯 PRIORITY 1: IMPLEMENTATION PLAN
## Following ASP.NET Core Best Practices

---

## STATUS: IN PROGRESS

### What Works ✅
1. **Trial Registration Flow** - Real EF Core, Identity, transactions
2. **Onboarding Wizard** - 96 questions across 12 steps  
3. **Rules Engine** - 5 derivation rules seeded
4. **Workspace Provisioning** - Idempotent, auto-creates resources
5. **Authentication** - ASP.NET Core Identity with JWT support
6. **Authorization** - Policy-based with custom handlers
7. **Multi-tenancy** - TenantId filtering via EF global filters
8. **Workflows** - 7 workflow definitions seeded

### Issues Found ⚠️
1. **Localization** - Resource keys showing instead of values (minor)
2. **Missing Views** - `/RoleProfile/Roles` and `/RoleProfile/Titles`
3. **Landing Page** - No dedicated marketing/landing page

---

## PHASE 1: UI & FUNCTIONALITY FIXES

### 1.1 Fix Missing Views (ASP.NET MVC Pattern)

Following the existing pattern in the codebase:
- Controllers in `Controllers/`
- Views in `Views/{ControllerName}/{ActionName}.cshtml`
- ViewModels in `Models/ViewModels/`

**Files to Create:**
- `Views/RoleProfile/Roles.cshtml`
- `Views/RoleProfile/Titles.cshtml`

### 1.2 Create Landing Page

Following ASP.NET MVC conventions:
- Create `LandingController.cs` with `[AllowAnonymous]`
- Create `Views/Landing/Index.cshtml`
- Update routing in `Program.cs` to make it the default for unauthenticated users

### 1.3 Fix Localization

The localization is configured but resources may not be loading properly.
Check:
- Resource file build action
- Namespace alignment between resource class and .resx files

---

## PHASE 2: KNOWLEDGE BASE / TOOLKIT

### 2.1 Structure (Following ASP.NET MVC)

```
Controllers/
  HelpController.cs         ← Already exists
  KnowledgeBaseController.cs ← NEW
  GuidanceController.cs      ← NEW

Views/
  KnowledgeBase/
    Index.cshtml
    Article.cshtml
    Category.cshtml
    Search.cshtml
  Guidance/
    GettingStarted.cshtml
    RoleGuide.cshtml
    WorkflowGuide.cshtml
    ControlOwnership.cshtml

Models/Entities/
  KnowledgeArticle.cs
  ArticleCategory.cs
```

### 2.2 Existing Help System

Already exists at:
- `Views/Help/Index.cshtml`
- `Views/Help/FAQ.cshtml`
- `Views/Help/GettingStarted.cshtml`
- `Views/Help/Glossary.cshtml`
- `Views/Help/Contact.cshtml`

**Enhance with:**
- Role-specific guidance
- Workflow documentation
- Control ownership matrix guide
- Evidence collection guide

---

## PHASE 3: PERMISSIONS SETUP

### 3.1 Existing Permission System

Located in:
- `Application/Permissions/GrcPermissions.cs`
- `Application/Permissions/PermissionDefinitionProvider.cs`
- `Application/Permissions/PermissionSeederService.cs`

### 3.2 Required Permissions (per module)

Already defined in `GrcPermissions.cs`:
- `Grc.Home`, `Grc.Dashboard`
- `Grc.Subscriptions.*` (View, Manage)
- `Grc.Admin.*` (Access, Users, Roles, Tenants)
- `Grc.Frameworks.*` (View, Create, Update, Delete, Import)
- `Grc.Regulators.*` (View, Manage)
- `Grc.Assessments.*` (View, Create, Update, Submit, Approve)
- `Grc.ControlAssessments.*` (View, Manage)
- `Grc.Evidence.*` (View, Upload, Update, Delete, Approve)
- `Grc.Risks.*` (View, Manage, Accept)
- `Grc.Audits.*` (View, Manage, Close)
- `Grc.ActionPlans.*` (View, Manage, Assign, Close)
- `Grc.Policies.*` (View, Manage, Approve, Publish)
- `Grc.ComplianceCalendar.*` (View, Manage)
- `Grc.Workflow.*` (View, Manage)
- `Grc.Notifications.*` (View, Manage)
- `Grc.Vendors.*` (View, Manage, Assess)
- `Grc.Reports.*` (View, Export)
- `Grc.Integrations.*` (View, Manage)

### 3.3 Seed Permissions to Database

Call `PermissionSeederService.SeedPermissionsAsync()` during app initialization.

---

## PHASE 4: DERIVATION RULES (50+)

### 4.1 Existing Rules

5 rules seeded in `SeedDataInitializer.cs`:
1. `RULE_COUNTRY_SA` - Country = SA
2. `RULE_SECTOR_FINANCIAL` - Banking/Insurance/FinTech → SAMA
3. `RULE_DATA_PROCESSING` - Personal data → PDPL
4. `RULE_CRITICAL_INFRA` - Energy/Telecom/Healthcare → NRA + MOI
5. `RULE_CLOUD_HOSTING` - Cloud/Hybrid → Cloud Security Package

### 4.2 Rules to Add

**By Regulator (15 rules):**
- NCA ECC (National Cybersecurity Authority)
- SAMA CSF (Saudi Central Bank)
- PDPL (Personal Data Protection)
- CCC (Cloud Computing Controls)
- CITC (Telecom regulator)
- MOH (Healthcare)
- ZATCA (Tax Authority)
- CMA (Capital Markets)
- MISA (Investment Authority)
- MCIT (Ministry of Communications)
- MOE (Education - if processing student data)
- SAGIA (Investment licensing)
- GACA (Aviation)
- NWC (Water)
- SEC (Electricity)

**By Sector (15 rules):**
- Banking → SAMA CSF + NCA ECC
- Insurance → SAMA + NCA
- FinTech → SAMA + PDPL + NCA
- Healthcare → MOH + NCA + PDPL
- Telecom → CITC + NCA
- Energy → SEC + NCA + Critical Infra
- Government → NCA + PDPL + MOI
- Education → MOE + PDPL
- Retail → PDPL + ZATCA
- Real Estate → RERA + PDPL
- Manufacturing → NCA + Industry-specific
- Transportation → GACA/MOT + NCA
- Media → MCIT + PDPL
- Sports → GASC + PDPL
- Tourism → MOT + PDPL

**By Data Type (10 rules):**
- PII Processing → PDPL
- Financial Data → SAMA data requirements
- Health Records → MOH + PDPL special category
- Children's Data → PDPL enhanced protections
- Biometric Data → PDPL + NCA
- Government Data → NCA classified handling
- Credit Card Data → PCI-DSS + SAMA
- Geolocation Data → PDPL + Telecom
- Employee Data → PDPL + Labor law
- Customer Data → PDPL + sector-specific

**By Infrastructure (10 rules):**
- Cloud (AWS) → NCA CCC + Data Residency
- Cloud (Azure) → NCA CCC + Data Residency
- Cloud (GCP) → NCA CCC + Data Residency
- Cloud (Alibaba) → NCA CCC + Data Residency
- On-Premise → NCA physical security
- Hybrid → Both cloud + on-prem rules
- Co-location (KSA) → NCA + physical
- Co-location (International) → Data transfer rules
- Edge Computing → IoT controls
- ICS/OT → Critical infrastructure controls

---

## PHASE 5: COMPLETE REGULATORS (130+)

### 5.1 Current: 92 Regulators

### 5.2 Missing (~40 Regulators)

**Local KSA (to add):**
- Saudi Central Bank (SAMA) - already exists, verify
- Capital Market Authority (CMA)
- Insurance Authority (IA) - previously SAMA
- National Cybersecurity Authority (NCA)
- Saudi Data & AI Authority (SDAIA)
- Communications, Space & Technology Commission (CITC)
- Ministry of Commerce (MOC)
- Ministry of Human Resources (MOHR)
- Ministry of Health (MOH)
- Ministry of Education (MOE)
- Ministry of Interior (MOI)
- Ministry of Finance (MOF)
- General Authority of Zakat and Tax (ZATCA)
- Saudi Food & Drug Authority (SFDA)
- General Authority of Civil Aviation (GACA)
- Saudi Ports Authority
- Saudi Railway Organization
- Saudi Electricity Company (SEC)
- National Water Company (NWC)
- Saudi Real Estate General Authority (REGA)
- General Commission for Audiovisual Media (GCAM)
- Saudi Sports Authority (GSA)

**International (to add):**
- ISO (various standards body)
- NIST (US standards)
- BSI (German standards)
- ENISA (EU cybersecurity agency)
- ICO (UK data protection)
- FTC (US consumer protection)
- SEC (US securities)
- FINRA (US financial)
- OCC (US banking)
- FDIC (US banking)
- PCI SSC (Payment cards)
- SWIFT (Banking network)
- Basel Committee (Banking)
- FATF (Anti-money laundering)
- Interpol (Law enforcement)

---

## PHASE 6: CONTROL-EVIDENCE MAPPING

### 6.1 Schema

Already exists:
- `ControlEvidencePack` - Links Control to EvidencePack
- `EvidencePack` - Groups evidence items
- `StandardEvidenceItem` - Individual evidence requirements

### 6.2 Mapping Approach

For each control, define:
1. Required evidence type(s)
2. Collection frequency
3. Retention period
4. Verification method
5. Scoring criteria

---

## PHASE 7: CONTROL DATA VALIDATION

### 7.1 Current: 57,212 lines in CSV

### 7.2 Validation Checks

1. **Completeness**: All required fields populated
2. **Consistency**: Codes follow naming convention
3. **Hierarchy**: Parent-child relationships valid
4. **Uniqueness**: No duplicate control codes
5. **References**: All framework references exist

---

## EXECUTION ORDER

1. ✅ Application running
2. 🔄 Fix localization (quick win)
3. 🔄 Create missing views
4. 🔄 Create landing page
5. 🔄 Seed permissions
6. 🔄 Add 45+ derivation rules
7. 🔄 Complete regulator list
8. 🔄 Create control-evidence mappings
9. 🔄 Validate control data
10. 🔄 Create knowledge base articles

---

## ASP.NET BEST PRACTICES FOLLOWED

1. **Dependency Injection** - All services registered in `Program.cs`
2. **Interface/Implementation** - `Services/Interfaces/` + `Services/Implementations/`
3. **Repository Pattern** - `IGenericRepository<T>` + `IUnitOfWork`
4. **Entity Framework Core** - `GrcDbContext` with migrations
5. **ASP.NET Core Identity** - `ApplicationUser`, `IdentityRole`
6. **Authorization Policies** - Custom handlers in `Authorization/`
7. **Validation** - FluentValidation in `Validators/`
8. **Localization** - `IStringLocalizer`, `.resx` files
9. **Configuration** - Options pattern with validation
10. **Logging** - Serilog with structured logging
11. **Health Checks** - `/health` endpoint
12. **Background Jobs** - Hangfire
13. **CSRF Protection** - `[ValidateAntiForgeryToken]`
14. **Rate Limiting** - Built-in rate limiter

---

## NEXT IMMEDIATE ACTIONS

1. Create `Views/RoleProfile/Roles.cshtml`
2. Create `Views/RoleProfile/Titles.cshtml`
3. Create `Controllers/LandingController.cs`
4. Create `Views/Landing/Index.cshtml`
5. Add 45 derivation rules to seed data
