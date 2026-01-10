# DATA INTEGRITY AND LOCALIZATION FIX GUIDE
**Shahin AI GRC Platform - Comprehensive Remediation Plan**
**Generated:** 2026-01-10
**Version:** 1.0

---

## EXECUTIVE SUMMARY

This guide provides a comprehensive remediation plan for **data integrity issues** and **localization key mismatches** identified in the Shahin AI GRC Platform. It includes automated tools, SQL scripts, and step-by-step instructions to resolve all identified issues.

### Issues Addressed

| Category | Issues Found | Severity | Status |
|----------|--------------|----------|--------|
| **Data Integrity** | Orphaned records due to SetNull behavior | HIGH | ✅ SQL audit queries provided |
| **Data Integrity** | Missing concurrency control (RowVersion) | MEDIUM | ✅ Migration ready |
| **Data Integrity** | Non-unique BusinessCode duplicates | MEDIUM | ✅ Migration ready |
| **Localization** | 15 EN keys missing AR translations | HIGH | ✅ PowerShell script ready |
| **Localization** | 1,200 orphaned AR keys | MEDIUM | ✅ PowerShell script ready |
| **Localization** | 336 API keys in separate files | MEDIUM | ✅ C# script ready |

---

## TABLE OF CONTENTS

1. [Data Integrity Issues](#1-data-integrity-issues)
   - 1.1 [Orphaned Records Detection](#11-orphaned-records-detection)
   - 1.2 [Missing Concurrency Control](#12-missing-concurrency-control)
   - 1.3 [Duplicate BusinessCodes](#13-duplicate-businesscodes)
2. [Localization Issues](#2-localization-issues)
   - 2.1 [Missing Arabic Translations](#21-missing-arabic-translations)
   - 2.2 [Orphaned Arabic Keys](#22-orphaned-arabic-keys)
   - 2.3 [API Resource Consolidation](#23-api-resource-consolidation)
3. [Remediation Roadmap](#3-remediation-roadmap)
4. [Testing & Validation](#4-testing--validation)
5. [Appendix](#5-appendix)

---

## 1. DATA INTEGRITY ISSUES

### 1.1 Orphaned Records Detection

**Problem:** Records with NULL foreign keys due to `DeleteBehavior.SetNull` causing orphaned relationships.

**Affected Entities:**
- Assessments (RiskId, ControlId)
- Evidences (AssessmentId, AuditId, ControlId, AssessmentRequirementId)
- Controls (RiskId)
- ControlImplementations (ControlId)
- Baselines (FrameworkId)

**Root Cause (from [GrcDbContext.cs](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs)):**

| Line | Relationship | Behavior |
|------|--------------|----------|
| 446 | Control.RiskId → Risk | SetNull |
| 462 | Assessment.RiskId → Risk | SetNull |
| 467 | Assessment.ControlId → Control | SetNull |
| 510 | Evidence.AssessmentId → Assessment | SetNull |
| 515 | Evidence.AuditId → Audit | SetNull |
| 520 | Evidence.ControlId → Control | SetNull |
| 525 | Evidence.AssessmentRequirementId → AssessmentRequirement | SetNull |
| 1206 | ControlImplementation.ControlId → Control | SetNull |
| 1236 | Baseline.FrameworkId → Framework | SetNull |

**Solution File:** [DATA_INTEGRITY_AUDIT_REPORT.sql](/home/Shahin-ai/Shahin-Jan-2026/DATA_INTEGRITY_AUDIT_REPORT.sql)

**How to Run:**

```bash
# Connect to PostgreSQL database
psql -h localhost -U postgres -d GrcMvcDb

# Run the audit script
\i DATA_INTEGRITY_AUDIT_REPORT.sql

# Or run specific sections
# Section 6: Summary statistics
\i DATA_INTEGRITY_AUDIT_REPORT.sql --section 6

# Section 7: Tenant-level breakdown
\i DATA_INTEGRITY_AUDIT_REPORT.sql --section 7
```

**Key Queries:**

1. **Summary Statistics** (Section 6):
   - Quick overview of orphaned record counts across all entities
   - Single query execution for dashboard view

2. **Orphaned Assessments** (Section 1):
   - Assessments with NULL RiskId
   - Assessments with NULL ControlId
   - Completely orphaned assessments (both NULL)

3. **Orphaned Evidences** (Section 2):
   - Evidence with NULL AssessmentId
   - Evidence with NULL AuditId
   - Evidence with NULL ControlId
   - Completely orphaned evidence (all NULL)

4. **Tenant-Level Breakdown** (Section 7):
   - Identifies which tenants have the most orphaned records
   - Useful for prioritizing remediation efforts

**Remediation Options:**

| Option | Description | Risk | Recommended For |
|--------|-------------|------|-----------------|
| **Re-link** | Manually re-link orphaned records to valid parents | Low | Small number of orphans (<50) |
| **Soft Delete** | Mark orphaned records as deleted | Medium | Large number of orphans (>50) |
| **Hard Delete** | Permanently remove orphaned records | High | Data cleanup after backup |
| **Convert to SetNull + Required** | Change relationships to require parent | Low | Future prevention |

**Example Remediation SQL:**

```sql
-- Option 1: Soft delete orphaned assessments
UPDATE "Assessments"
SET "IsDeleted" = TRUE,
    "DeletedAt" = NOW(),
    "ModifiedBy" = 'SYSTEM_CLEANUP',
    "ModifiedDate" = NOW()
WHERE "RiskId" IS NULL
  AND "ControlId" IS NULL
  AND "IsDeleted" = FALSE;

-- Option 2: Re-link to a default "Orphaned Items" parent
-- First create a placeholder Risk
INSERT INTO "Risks" ("Id", "Name", "Category", "TenantId", "CreatedDate", "CreatedBy")
VALUES (gen_random_uuid(), 'Orphaned Items Container', 'System', NULL, NOW(), 'SYSTEM');

-- Then re-link orphaned assessments
UPDATE "Assessments"
SET "RiskId" = (SELECT "Id" FROM "Risks" WHERE "Name" = 'Orphaned Items Container'),
    "ModifiedBy" = 'SYSTEM_CLEANUP',
    "ModifiedDate" = NOW()
WHERE "RiskId" IS NULL
  AND "IsDeleted" = FALSE;
```

---

### 1.2 Missing Concurrency Control

**Problem:** Only the `Risk` entity has `RowVersion` for optimistic concurrency control. Frequently-updated entities lack this protection, risking lost updates in concurrent editing scenarios.

**Affected Entities (Missing RowVersion):**
- Assessment
- Control
- Evidence
- Audit
- AuditFinding
- Policy
- PolicyViolation
- Workflow
- WorkflowExecution
- AssessmentRequirement
- ControlImplementation
- GapClosurePlan
- Tenant
- Workspace

**Solution File:** [20260110_AddDataIntegrityConstraints.cs](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/Migrations/20260110_AddDataIntegrityConstraints.cs)

**How to Apply:**

```bash
# Navigate to project directory
cd /home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc

# Add the migration to your migrations folder
# (File already created at correct location)

# Apply the migration
dotnet ef database update --context GrcDbContext

# Verify migration
dotnet ef migrations list --context GrcDbContext
```

**What the Migration Does:**

1. **Adds RowVersion columns** to 14 frequently-updated entities
2. **Creates unique BusinessCode constraints** for all core entities
3. **Adds orphaned record detection indexes** for performance
4. **Adds check constraints** for data validation:
   - CreatedDate not in future
   - ModifiedDate >= CreatedDate
   - DeletedAt consistency with IsDeleted flag
   - FileSize > 0 for Evidence

**Benefits:**

- **Prevents lost updates** when multiple users edit the same record
- **Database-level validation** catches data anomalies early
- **Improved query performance** for orphaned record detection
- **Business rule enforcement** at database level (defense in depth)

**Testing Concurrency:**

```csharp
// Example: Test concurrency control on Assessment
var assessment1 = await context.Assessments.FindAsync(assessmentId);
var assessment2 = await context.Assessments.FindAsync(assessmentId);

// User 1 updates
assessment1.Status = "In Progress";
await context.SaveChangesAsync(); // Success

// User 2 tries to update (will throw DbUpdateConcurrencyException)
assessment2.Status = "Completed";
await context.SaveChangesAsync(); // EXCEPTION - RowVersion mismatch
```

---

### 1.3 Duplicate BusinessCodes

**Problem:** `BusinessCode` is an immutable, human-readable identifier (serial number) that should be unique but lacks database constraint. This allows duplicates to exist.

**Detection Query (from audit report Section 10):**

```sql
-- Find duplicate BusinessCodes in Assessments
SELECT
    "BusinessCode",
    COUNT(*) AS DuplicateCount,
    STRING_AGG("Id"::TEXT, ', ') AS AffectedIds
FROM "Assessments"
WHERE "BusinessCode" IS NOT NULL
  AND "IsDeleted" = FALSE
GROUP BY "BusinessCode"
HAVING COUNT(*) > 1;
```

**Solution:** The migration [20260110_AddDataIntegrityConstraints.cs](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/Migrations/20260110_AddDataIntegrityConstraints.cs) adds unique constraints:

```csharp
// Example unique constraint (partial index - ignores NULL and deleted)
migrationBuilder.CreateIndex(
    name: "UK_Assessments_BusinessCode",
    table: "Assessments",
    column: "BusinessCode",
    unique: true,
    filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE"
);
```

**Before Migration - Fix Existing Duplicates:**

```sql
-- Step 1: Identify duplicates
WITH Duplicates AS (
    SELECT "BusinessCode", ROW_NUMBER() OVER (PARTITION BY "BusinessCode" ORDER BY "CreatedDate") AS RowNum
    FROM "Assessments"
    WHERE "BusinessCode" IS NOT NULL AND "IsDeleted" = FALSE
)
SELECT * FROM Duplicates WHERE RowNum > 1;

-- Step 2: Assign new BusinessCode to duplicates (keep oldest, renumber rest)
UPDATE "Assessments" a
SET "BusinessCode" = CONCAT(a."BusinessCode", '-DUP-', d.RowNum),
    "ModifiedBy" = 'SYSTEM_CLEANUP',
    "ModifiedDate" = NOW()
FROM (
    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "BusinessCode" ORDER BY "CreatedDate") AS RowNum
    FROM "Assessments"
    WHERE "BusinessCode" IS NOT NULL AND "IsDeleted" = FALSE
) d
WHERE a."Id" = d."Id" AND d.RowNum > 1;
```

---

## 2. LOCALIZATION ISSUES

### 2.1 Missing Arabic Translations

**Problem:** 15 English localization keys have no corresponding Arabic translations, causing UI to display untranslated keys in Arabic mode.

**Affected Keys:**
```
Analytics
APPLY_Scope
AssessmentTemplates
AuditPackages
CatalogAdmin
CCMTests
ContactSupport
ControlMatrix
DocumentFlow
EvidenceLifecycle
EvidenceStatistics
Exceptions
ExecutiveDashboard
FAQ
Feedback
```

**Solution File:** [LocalizationReconciliation.ps1](/home/Shahin-ai/Shahin-Jan-2026/LocalizationReconciliation.ps1)

**How to Run:**

```bash
# Option 1: Generate report only (no changes)
pwsh LocalizationReconciliation.ps1 -GenerateReport

# Option 2: Generate report + add placeholder AR translations
pwsh LocalizationReconciliation.ps1 -GenerateReport -FixMismatches

# After running with -FixMismatches:
# 1. Open SharedResource.ar.resx
# 2. Search for "[NEEDS TRANSLATION]"
# 3. Replace with actual Arabic translations
```

**Manual Translation Template:**

| EN Key | EN Value | AR Value (To Translate) |
|--------|----------|-------------------------|
| Analytics | Analytics | التحليلات |
| ContactSupport | Contact Support | اتصل بالدعم |
| ExecutiveDashboard | Executive Dashboard | لوحة المعلومات التنفيذية |
| ... | ... | ... |

**Validation:**

```bash
# After translating, verify no "[NEEDS TRANSLATION]" remains
grep -r "\[NEEDS TRANSLATION\]" src/GrcMvc/Resources/SharedResource.ar.resx
# Should return no results
```

---

### 2.2 Orphaned Arabic Keys

**Problem:** 1,200 Arabic keys have no corresponding English source, likely duplicates or variants. This bloats the resource file and causes maintenance confusion.

**Solution File:** [LocalizationReconciliation.ps1](/home/Shahin-ai/Shahin-Jan-2026/LocalizationReconciliation.ps1)

**How to Run:**

```bash
# Generate report to review orphaned keys first
pwsh LocalizationReconciliation.ps1 -GenerateReport

# Review LOCALIZATION_RECONCILIATION_REPORT.md
# Confirm orphaned keys are safe to remove

# Remove orphaned AR keys
pwsh LocalizationReconciliation.ps1 -RemoveDuplicates

# Verify AR key count matches EN key count
# EN: 1,215 keys → AR: should also be ~1,215 keys (plus new API keys)
```

**Risk Assessment:**

| Risk Level | Description | Mitigation |
|------------|-------------|------------|
| LOW | Most orphaned keys are duplicates/variants | Script creates backup before removal |
| MEDIUM | Some keys might be used in legacy code | Test application thoroughly after removal |
| HIGH | Keys might be referenced in database strings | Search codebase for key names before removal |

**Pre-Removal Validation:**

```bash
# Search codebase for orphaned keys usage
cd /home/Shahin-ai/Shahin-Jan-2026

# Example: Check if "OrphanedKeyName" is used anywhere
grep -r "OrphanedKeyName" src/ --include="*.cs" --include="*.cshtml"
grep -r "OrphanedKeyName" grc-frontend/ --include="*.ts" --include="*.tsx" --include="*.js"
```

---

### 2.3 API Resource Consolidation

**Problem:** 336 API-specific localization keys are in separate XML files (`api-i18n-resources-{lang}.xml`) instead of main `SharedResource.{lang}.resx`. This creates:
- **Duplicate maintenance** burden
- **Inconsistent** key naming patterns
- **Deployment complexity** (multiple resource files)

**Solution File:** [ConsolidateApiResources.csx](/home/Shahin-ai/Shahin-Jan-2026/ConsolidateApiResources.csx)

**How to Run:**

```bash
# Install dotnet-script (if not already installed)
dotnet tool install -g dotnet-script

# Run consolidation script
dotnet script ConsolidateApiResources.csx

# Review consolidation report
cat API_RESOURCE_CONSOLIDATION_REPORT.md
```

**What the Script Does:**

1. **Parses** both API resource files (EN + AR)
2. **Identifies** 336 API keys not in main SharedResource files
3. **Creates backups** of SharedResource.en.resx and SharedResource.ar.resx
4. **Merges** API keys into main resource files with comment markers
5. **Generates** detailed consolidation report

**Post-Consolidation Actions:**

```bash
# 1. Verify merged keys
grep "Merged from api-i18n-resources" src/GrcMvc/Resources/SharedResource.en.resx

# 2. Archive old API resource files
mkdir -p archive/api-resources-20260110
mv api-i18n-resources-en.xml archive/api-resources-20260110/
mv api-i18n-resources-ar.xml archive/api-resources-20260110/

# 3. Update code references (if any)
# Search for any code loading api-i18n-resources-*.xml
grep -r "api-i18n-resources" src/ --include="*.cs"

# 4. Test API localization
# Run application and test API endpoints in both EN and AR modes
```

---

## 3. REMEDIATION ROADMAP

### Phase 1: Assessment & Backup (Week 1)

**Priority: CRITICAL**

| Task | Tool/File | Time | Owner |
|------|-----------|------|-------|
| Run data integrity audit | [DATA_INTEGRITY_AUDIT_REPORT.sql](/home/Shahin-ai/Shahin-Jan-2026/DATA_INTEGRITY_AUDIT_REPORT.sql) | 1 hour | DBA |
| Generate localization report | [LocalizationReconciliation.ps1](/home/Shahin-ai/Shahin-Jan-2026/LocalizationReconciliation.ps1) | 30 min | Dev Lead |
| Backup database | `pg_dump GrcMvcDb > backup_pre_remediation.sql` | 15 min | DBA |
| Review audit results | Manual analysis | 2 hours | Team |
| Prioritize orphaned records | Based on tenant impact | 1 hour | PM |

**Success Criteria:**
- ✅ Full database backup completed
- ✅ Audit report generated for all 7 tenants
- ✅ Orphaned record counts documented
- ✅ Localization gaps identified (15 EN-only, 1200 AR-only, 336 API keys)

---

### Phase 2: Data Integrity Fixes (Week 2)

**Priority: HIGH**

| Task | Tool/File | Time | Owner |
|------|-----------|------|-------|
| Fix duplicate BusinessCodes | SQL remediation script | 2 hours | DBA |
| Apply database constraints migration | [20260110_AddDataIntegrityConstraints.cs](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/Migrations/20260110_AddDataIntegrityConstraints.cs) | 1 hour | Dev |
| Test concurrency control | Manual testing | 2 hours | QA |
| Re-link or soft-delete orphaned records | Tenant-specific SQL scripts | 4 hours | DBA + PM |
| Verify referential integrity | Re-run audit report | 30 min | DBA |

**Success Criteria:**
- ✅ Zero duplicate BusinessCodes
- ✅ RowVersion added to 14 entities
- ✅ Unique constraints enforced
- ✅ Orphaned records reduced by 80%+
- ✅ All check constraints active

---

### Phase 3: Localization Fixes (Week 3)

**Priority: MEDIUM**

| Task | Tool/File | Time | Owner |
|------|-----------|------|-------|
| Add placeholder AR translations | [LocalizationReconciliation.ps1](/home/Shahin-ai/Shahin-Jan-2026/LocalizationReconciliation.ps1) `-FixMismatches` | 15 min | Dev |
| Translate 15 EN-only keys to Arabic | Manual translation | 2 hours | Translator |
| Remove 1,200 orphaned AR keys | [LocalizationReconciliation.ps1](/home/Shahin-ai/Shahin-Jan-2026/LocalizationReconciliation.ps1) `-RemoveDuplicates` | 15 min | Dev |
| Consolidate 336 API keys | [ConsolidateApiResources.csx](/home/Shahin-ai/Shahin-Jan-2026/ConsolidateApiResources.csx) | 30 min | Dev |
| Test UI in EN and AR modes | Manual testing | 3 hours | QA |
| Archive old API resource files | `mkdir archive/` | 5 min | Dev |

**Success Criteria:**
- ✅ EN and AR key counts match (1,551 each = 1,215 + 336 API)
- ✅ Zero "[NEEDS TRANSLATION]" placeholders
- ✅ All API responses localized correctly
- ✅ Old API resource files archived
- ✅ UI displays Arabic correctly (RTL, no broken keys)

---

### Phase 4: Validation & Documentation (Week 4)

**Priority: MEDIUM**

| Task | Time | Owner |
|------|------|-------|
| Re-run all audit queries (verify fixes) | 1 hour | DBA |
| Load test concurrent editing scenarios | 4 hours | QA |
| Regression testing (all modules) | 8 hours | QA |
| Update GrcDbContext.cs with RowVersion config | 1 hour | Dev |
| Document BusinessCode serial generation logic | 2 hours | Dev |
| Update localization documentation | 1 hour | Tech Writer |
| Create runbook for future orphaned record detection | 2 hours | DBA |

**Success Criteria:**
- ✅ All audit queries return zero critical issues
- ✅ Concurrency exceptions properly handled in UI
- ✅ No regression bugs found
- ✅ Documentation updated in Wiki
- ✅ Runbooks created for DBA and Dev teams

---

## 4. TESTING & VALIDATION

### 4.1 Data Integrity Testing

**Test Case 1: Concurrent Edit Protection**

```csharp
// Test: Two users editing same assessment simultaneously
[Fact]
public async Task ConcurrentEdit_ThrowsDbUpdateConcurrencyException()
{
    // Arrange
    var assessmentId = Guid.NewGuid();
    var user1Context = CreateDbContext();
    var user2Context = CreateDbContext();

    // Act
    var assessment1 = await user1Context.Assessments.FindAsync(assessmentId);
    var assessment2 = await user2Context.Assessments.FindAsync(assessmentId);

    assessment1.Status = "In Progress";
    await user1Context.SaveChangesAsync(); // Success

    assessment2.Status = "Completed";

    // Assert
    await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
        async () => await user2Context.SaveChangesAsync()
    );
}
```

**Test Case 2: BusinessCode Uniqueness**

```csharp
// Test: Duplicate BusinessCode insertion fails
[Fact]
public async Task DuplicateBusinessCode_ThrowsDbUpdateException()
{
    // Arrange
    var context = CreateDbContext();
    var businessCode = "ASM-2026-001";

    var assessment1 = new Assessment { BusinessCode = businessCode, Name = "Assessment 1" };
    var assessment2 = new Assessment { BusinessCode = businessCode, Name = "Assessment 2" };

    // Act
    context.Assessments.Add(assessment1);
    await context.SaveChangesAsync(); // Success

    context.Assessments.Add(assessment2);

    // Assert
    await Assert.ThrowsAsync<DbUpdateException>(
        async () => await context.SaveChangesAsync()
    );
}
```

**Test Case 3: Orphaned Record Detection**

```sql
-- Test: Verify orphaned records query performance
EXPLAIN ANALYZE
SELECT COUNT(*)
FROM "Assessments"
WHERE "RiskId" IS NULL AND "IsDeleted" = FALSE;

-- Expected: Index scan on IX_Assessments_RiskId_IsDeleted
-- Execution time: <50ms
```

---

### 4.2 Localization Testing

**Test Case 1: All UI Keys Translated**

```bash
# Test: No missing localization keys in Arabic mode
# 1. Set application to Arabic locale
# 2. Navigate through all UI screens
# 3. Search for:
#    - Raw key names (e.g., "Assessment_Create_New")
#    - "[NEEDS TRANSLATION]" placeholders
#    - English text in Arabic mode

# Expected: Zero untranslated keys
```

**Test Case 2: API Response Localization**

```csharp
// Test: API returns localized error messages
[Theory]
[InlineData("en", "Assessment not found")]
[InlineData("ar", "التقييم غير موجود")]
public async Task ApiError_ReturnsLocalizedMessage(string locale, string expectedMessage)
{
    // Arrange
    var client = CreateHttpClient();
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(locale);

    // Act
    var response = await client.GetAsync("/api/assessments/invalid-id");
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(expectedMessage, content);
}
```

**Test Case 3: Resource File Consistency**

```powershell
# Test: EN and AR key counts match
$enKeys = ([xml](Get-Content SharedResource.en.resx)).root.data.Count
$arKeys = ([xml](Get-Content SharedResource.ar.resx)).root.data.Count

Write-Host "EN Keys: $enKeys"
Write-Host "AR Keys: $arKeys"

# Expected: $enKeys == $arKeys (±5 tolerance for ongoing development)
if ([Math]::Abs($enKeys - $arKeys) -gt 5) {
    throw "Key count mismatch exceeds tolerance"
}
```

---

## 5. APPENDIX

### A. File Reference

| File | Purpose | Location |
|------|---------|----------|
| DATA_INTEGRITY_AUDIT_REPORT.sql | SQL queries to detect orphaned records | [/home/Shahin-ai/Shahin-Jan-2026/](/home/Shahin-ai/Shahin-Jan-2026/DATA_INTEGRITY_AUDIT_REPORT.sql) |
| LocalizationReconciliation.ps1 | PowerShell script to sync EN/AR keys | [/home/Shahin-ai/Shahin-Jan-2026/](/home/Shahin-ai/Shahin-Jan-2026/LocalizationReconciliation.ps1) |
| ConsolidateApiResources.csx | C# script to merge API resources | [/home/Shahin-ai/Shahin-Jan-2026/](/home/Shahin-ai/Shahin-Jan-2026/ConsolidateApiResources.csx) |
| 20260110_AddDataIntegrityConstraints.cs | EF Core migration for constraints | [/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/Migrations/](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/Migrations/20260110_AddDataIntegrityConstraints.cs) |
| GrcDbContext.cs | Main DbContext with entity configurations | [/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs) |

---

### B. Database Connection Strings

```json
// From appsettings.json
{
  "ConnectionStrings": {
    "GrcMvcDb": "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD",
    "GrcAuthDb": "Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

---

### C. Useful Commands

**Database Backup:**
```bash
# Full backup
pg_dump -h localhost -U postgres GrcMvcDb > backup_$(date +%Y%m%d).sql

# Schema only
pg_dump -h localhost -U postgres --schema-only GrcMvcDb > schema_$(date +%Y%m%d).sql

# Data only
pg_dump -h localhost -U postgres --data-only GrcMvcDb > data_$(date +%Y%m%d).sql
```

**Migration Commands:**
```bash
# List migrations
dotnet ef migrations list --context GrcDbContext

# Add new migration
dotnet ef migrations add MigrationName --context GrcDbContext

# Apply migrations
dotnet ef database update --context GrcDbContext

# Rollback last migration
dotnet ef database update PreviousMigrationName --context GrcDbContext

# Generate SQL script for migration
dotnet ef migrations script --context GrcDbContext --output migration.sql
```

**Resource File Validation:**
```powershell
# Count keys in resource files
([xml](Get-Content SharedResource.en.resx)).root.data.Count
([xml](Get-Content SharedResource.ar.resx)).root.data.Count

# Find keys with specific prefix
([xml](Get-Content SharedResource.en.resx)).root.data | Where-Object { $_.name -like "Api_*" }

# Extract all key names
([xml](Get-Content SharedResource.en.resx)).root.data.name | Sort-Object
```

---

### D. DeleteBehavior Reference

| Behavior | Description | Use Case |
|----------|-------------|----------|
| **Cascade** | Delete child when parent deleted | Audit → AuditFinding (finding requires audit) |
| **SetNull** | Set FK to NULL when parent deleted | Assessment → Risk (assessment can exist without risk) |
| **Restrict** | Prevent parent deletion if children exist | Tenant → Workspace (protect active relationships) |
| **NoAction** | No automatic action (app handles it) | Custom business logic required |

**Current Usage in GrcDbContext.cs:**
- **Cascade:** 14 relationships (tight coupling)
- **SetNull:** 24 relationships (loose coupling - **orphan risk**)
- **Restrict:** 1 relationship (protection)

---

### E. Localization Key Naming Convention

**Recommended Pattern:**

```
{Module}_{Screen}_{Element}_{Type}

Examples:
- Assessment_List_Title                    → Assessment module, list screen, title element
- Assessment_Create_Button_Save            → Assessment create screen, save button
- Api_Error_AssessmentNotFound             → API error message
- Common_Validation_RequiredField          → Common validation message
```

**Prefixes:**
- `Api_` - API responses
- `Common_` - Shared across modules
- `Validation_` - Form validation messages
- `Error_` - Error messages
- `Success_` - Success messages
- `{ModuleName}_` - Module-specific keys

---

### F. Contact & Support

| Role | Responsibility | Contact |
|------|----------------|---------|
| **DBA** | Database migrations, backup/restore | dba@shahin-ai.com |
| **Dev Lead** | Code changes, localization | dev@shahin-ai.com |
| **QA Lead** | Testing, validation | qa@shahin-ai.com |
| **Product Manager** | Business decisions, prioritization | pm@shahin-ai.com |

---

## CONCLUSION

This comprehensive guide provides all necessary tools and procedures to remediate data integrity and localization issues in the Shahin AI GRC Platform. Follow the phased roadmap to ensure a smooth, low-risk remediation process.

**Estimated Total Effort:** 4 weeks (1 week per phase)

**Risk Level:** LOW (with proper backups and testing)

**Business Impact:** HIGH (improved data quality, user experience, and system stability)

---

**Document Version:** 1.0
**Last Updated:** 2026-01-10
**Next Review:** 2026-02-10 (post-remediation)
