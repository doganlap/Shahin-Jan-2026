-- ========================================================================================================
-- DATA INTEGRITY AUDIT REPORT - ORPHANED RECORDS DETECTION
-- Generated: 2026-01-10
-- Purpose: Identify orphaned records caused by DeleteBehavior.SetNull relationships
-- ========================================================================================================

-- ========================================================================================================
-- SECTION 1: ORPHANED ASSESSMENTS
-- ========================================================================================================

-- 1.1 Assessments with NULL RiskId (orphaned from Risk)
-- Impact: Assessment exists but has no associated Risk
-- Root Cause: Risk was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:462)
SELECT
    'Orphaned_Assessment_NoRisk' AS IssueType,
    a."Id",
    a."AssessmentNumber",
    a."Name",
    a."Type",
    a."Status",
    a."RiskId" AS OrphanedRiskId,
    a."ControlId",
    a."TenantId",
    a."CreatedDate",
    a."CreatedBy",
    a."IsDeleted"
FROM "Assessments" a
WHERE a."RiskId" IS NULL
  AND a."IsDeleted" = FALSE
ORDER BY a."CreatedDate" DESC;

-- 1.2 Assessments with NULL ControlId (orphaned from Control)
-- Impact: Assessment exists but has no associated Control
-- Root Cause: Control was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:467)
SELECT
    'Orphaned_Assessment_NoControl' AS IssueType,
    a."Id",
    a."AssessmentNumber",
    a."Name",
    a."Type",
    a."Status",
    a."RiskId",
    a."ControlId" AS OrphanedControlId,
    a."TenantId",
    a."CreatedDate",
    a."CreatedBy"
FROM "Assessments" a
WHERE a."ControlId" IS NULL
  AND a."IsDeleted" = FALSE
ORDER BY a."CreatedDate" DESC;

-- 1.3 Assessments orphaned from BOTH Risk AND Control
-- Impact: Completely orphaned assessments with no parent entities
-- Severity: CRITICAL - assessment has lost all context
SELECT
    'Orphaned_Assessment_Complete' AS IssueType,
    a."Id",
    a."AssessmentNumber",
    a."Name",
    a."Type",
    a."Status",
    a."TenantId",
    a."CreatedDate",
    COUNT(ev."Id") AS EvidenceCount,
    COUNT(ar."Id") AS RequirementCount
FROM "Assessments" a
LEFT JOIN "Evidences" ev ON ev."AssessmentId" = a."Id" AND ev."IsDeleted" = FALSE
LEFT JOIN "AssessmentRequirements" ar ON ar."AssessmentId" = a."Id" AND ar."IsDeleted" = FALSE
WHERE a."RiskId" IS NULL
  AND a."ControlId" IS NULL
  AND a."IsDeleted" = FALSE
GROUP BY a."Id", a."AssessmentNumber", a."Name", a."Type", a."Status", a."TenantId", a."CreatedDate"
ORDER BY a."CreatedDate" DESC;


-- ========================================================================================================
-- SECTION 2: ORPHANED EVIDENCES
-- ========================================================================================================

-- 2.1 Evidences with NULL AssessmentId (orphaned from Assessment)
-- Impact: Evidence exists but has no associated Assessment
-- Root Cause: Assessment was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:510)
SELECT
    'Orphaned_Evidence_NoAssessment' AS IssueType,
    e."Id",
    e."EvidenceNumber",
    e."Title",
    e."FileName",
    e."FileSize",
    e."UploadDate",
    e."AssessmentId" AS OrphanedAssessmentId,
    e."AuditId",
    e."ControlId",
    e."AssessmentRequirementId",
    e."TenantId",
    e."CreatedBy"
FROM "Evidences" e
WHERE e."AssessmentId" IS NULL
  AND e."IsDeleted" = FALSE
ORDER BY e."UploadDate" DESC;

-- 2.2 Evidences with NULL AuditId (orphaned from Audit)
-- Impact: Evidence exists but has no associated Audit
-- Root Cause: Audit was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:515)
SELECT
    'Orphaned_Evidence_NoAudit' AS IssueType,
    e."Id",
    e."EvidenceNumber",
    e."Title",
    e."FileName",
    e."FileSize",
    e."UploadDate",
    e."AssessmentId",
    e."AuditId" AS OrphanedAuditId,
    e."ControlId",
    e."TenantId"
FROM "Evidences" e
WHERE e."AuditId" IS NULL
  AND e."IsDeleted" = FALSE
ORDER BY e."UploadDate" DESC;

-- 2.3 Evidences with NULL ControlId (orphaned from Control)
-- Impact: Evidence exists but has no associated Control
-- Root Cause: Control was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:520)
SELECT
    'Orphaned_Evidence_NoControl' AS IssueType,
    e."Id",
    e."EvidenceNumber",
    e."Title",
    e."FileName",
    e."FileSize",
    e."UploadDate",
    e."AssessmentId",
    e."AuditId",
    e."ControlId" AS OrphanedControlId,
    e."TenantId"
FROM "Evidences" e
WHERE e."ControlId" IS NULL
  AND e."IsDeleted" = FALSE
ORDER BY e."UploadDate" DESC;

-- 2.4 Evidences with NULL AssessmentRequirementId (orphaned from AssessmentRequirement)
-- Impact: Evidence exists but has no associated AssessmentRequirement
-- Root Cause: AssessmentRequirement was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:525)
SELECT
    'Orphaned_Evidence_NoRequirement' AS IssueType,
    e."Id",
    e."EvidenceNumber",
    e."Title",
    e."FileName",
    e."FileSize",
    e."UploadDate",
    e."AssessmentId",
    e."AssessmentRequirementId" AS OrphanedRequirementId,
    e."TenantId"
FROM "Evidences" e
WHERE e."AssessmentRequirementId" IS NULL
  AND e."IsDeleted" = FALSE
ORDER BY e."UploadDate" DESC;

-- 2.5 Completely orphaned Evidences (ALL foreign keys are NULL)
-- Severity: CRITICAL - evidence has lost all context
SELECT
    'Orphaned_Evidence_Complete' AS IssueType,
    e."Id",
    e."EvidenceNumber",
    e."Title",
    e."FileName",
    e."FileSize",
    e."UploadDate",
    e."TenantId",
    e."CreatedBy",
    e."CreatedDate"
FROM "Evidences" e
WHERE e."AssessmentId" IS NULL
  AND e."AuditId" IS NULL
  AND e."ControlId" IS NULL
  AND e."AssessmentRequirementId" IS NULL
  AND e."IsDeleted" = FALSE
ORDER BY e."UploadDate" DESC;


-- ========================================================================================================
-- SECTION 3: ORPHANED CONTROLS
-- ========================================================================================================

-- 3.1 Controls with NULL RiskId (orphaned from Risk)
-- Impact: Control exists but has no associated Risk
-- Root Cause: Risk was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:446)
SELECT
    'Orphaned_Control_NoRisk' AS IssueType,
    c."Id",
    c."ControlId" AS ControlCode,
    c."Name",
    c."Category",
    c."Type",
    c."Frequency",
    c."Status",
    c."RiskId" AS OrphanedRiskId,
    c."TenantId",
    c."CreatedDate",
    COUNT(DISTINCT a."Id") AS AssessmentCount,
    COUNT(DISTINCT e."Id") AS EvidenceCount
FROM "Controls" c
LEFT JOIN "Assessments" a ON a."ControlId" = c."Id" AND a."IsDeleted" = FALSE
LEFT JOIN "Evidences" e ON e."ControlId" = c."Id" AND e."IsDeleted" = FALSE
WHERE c."RiskId" IS NULL
  AND c."IsDeleted" = FALSE
GROUP BY c."Id", c."ControlId", c."Name", c."Category", c."Type", c."Frequency", c."Status", c."RiskId", c."TenantId", c."CreatedDate"
ORDER BY c."CreatedDate" DESC;


-- ========================================================================================================
-- SECTION 4: ORPHANED CONTROL IMPLEMENTATIONS
-- ========================================================================================================

-- 4.1 ControlImplementations with NULL ControlId (orphaned from Control)
-- Impact: ControlImplementation exists but has no associated Control
-- Root Cause: Control was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:1206)
SELECT
    'Orphaned_ControlImplementation_NoControl' AS IssueType,
    ci."Id",
    ci."ControlId" AS OrphanedControlId,
    ci."ImplementationStatus",
    ci."ImplementationDate",
    ci."TenantId",
    ci."WorkspaceId",
    ci."CreatedDate",
    ci."CreatedBy"
FROM "ControlImplementations" ci
WHERE ci."ControlId" IS NULL
  AND ci."IsDeleted" = FALSE
ORDER BY ci."CreatedDate" DESC;


-- ========================================================================================================
-- SECTION 5: ORPHANED BASELINES
-- ========================================================================================================

-- 5.1 Baselines with NULL FrameworkId (orphaned from Framework)
-- Impact: Baseline exists but has no associated Framework
-- Root Cause: Framework was deleted with DeleteBehavior.SetNull (GrcDbContext.cs:1236)
SELECT
    'Orphaned_Baseline_NoFramework' AS IssueType,
    b."Id",
    b."Name",
    b."Version",
    b."FrameworkId" AS OrphanedFrameworkId,
    b."TenantId",
    b."CreatedDate",
    b."CreatedBy"
FROM "Baselines" b
WHERE b."FrameworkId" IS NULL
  AND b."IsDeleted" = FALSE
ORDER BY b."CreatedDate" DESC;


-- ========================================================================================================
-- SECTION 6: SUMMARY STATISTICS
-- ========================================================================================================

-- 6.1 Orphaned Records Count Summary
SELECT
    'SUMMARY' AS ReportSection,
    (SELECT COUNT(*) FROM "Assessments" WHERE "RiskId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedAssessments_NoRisk,
    (SELECT COUNT(*) FROM "Assessments" WHERE "ControlId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedAssessments_NoControl,
    (SELECT COUNT(*) FROM "Assessments" WHERE "RiskId" IS NULL AND "ControlId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedAssessments_Complete,
    (SELECT COUNT(*) FROM "Evidences" WHERE "AssessmentId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedEvidences_NoAssessment,
    (SELECT COUNT(*) FROM "Evidences" WHERE "AuditId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedEvidences_NoAudit,
    (SELECT COUNT(*) FROM "Evidences" WHERE "ControlId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedEvidences_NoControl,
    (SELECT COUNT(*) FROM "Evidences" WHERE "AssessmentRequirementId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedEvidences_NoRequirement,
    (SELECT COUNT(*) FROM "Evidences" WHERE "AssessmentId" IS NULL AND "AuditId" IS NULL AND "ControlId" IS NULL AND "AssessmentRequirementId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedEvidences_Complete,
    (SELECT COUNT(*) FROM "Controls" WHERE "RiskId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedControls_NoRisk,
    (SELECT COUNT(*) FROM "ControlImplementations" WHERE "ControlId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedControlImplementations,
    (SELECT COUNT(*) FROM "Baselines" WHERE "FrameworkId" IS NULL AND "IsDeleted" = FALSE) AS OrphanedBaselines;


-- ========================================================================================================
-- SECTION 7: TENANT-LEVEL ORPHANED RECORDS BREAKDOWN
-- ========================================================================================================

-- 7.1 Orphaned records grouped by Tenant
SELECT
    t."Id" AS TenantId,
    t."Name" AS TenantName,
    t."CompanyName",
    COUNT(DISTINCT CASE WHEN a."RiskId" IS NULL THEN a."Id" END) AS OrphanedAssessments_NoRisk,
    COUNT(DISTINCT CASE WHEN a."ControlId" IS NULL THEN a."Id" END) AS OrphanedAssessments_NoControl,
    COUNT(DISTINCT CASE WHEN e."AssessmentId" IS NULL THEN e."Id" END) AS OrphanedEvidences_NoAssessment,
    COUNT(DISTINCT CASE WHEN e."AuditId" IS NULL THEN e."Id" END) AS OrphanedEvidences_NoAudit,
    COUNT(DISTINCT CASE WHEN c."RiskId" IS NULL THEN c."Id" END) AS OrphanedControls_NoRisk
FROM "Tenants" t
LEFT JOIN "Assessments" a ON a."TenantId" = t."Id" AND a."IsDeleted" = FALSE
LEFT JOIN "Evidences" e ON e."TenantId" = t."Id" AND e."IsDeleted" = FALSE
LEFT JOIN "Controls" c ON c."TenantId" = t."Id" AND c."IsDeleted" = FALSE
WHERE t."IsActive" = TRUE
GROUP BY t."Id", t."Name", t."CompanyName"
HAVING COUNT(DISTINCT CASE WHEN a."RiskId" IS NULL THEN a."Id" END) > 0
    OR COUNT(DISTINCT CASE WHEN a."ControlId" IS NULL THEN a."Id" END) > 0
    OR COUNT(DISTINCT CASE WHEN e."AssessmentId" IS NULL THEN e."Id" END) > 0
    OR COUNT(DISTINCT CASE WHEN e."AuditId" IS NULL THEN e."Id" END) > 0
    OR COUNT(DISTINCT CASE WHEN c."RiskId" IS NULL THEN c."Id" END) > 0
ORDER BY
    (COUNT(DISTINCT CASE WHEN a."RiskId" IS NULL THEN a."Id" END) +
     COUNT(DISTINCT CASE WHEN a."ControlId" IS NULL THEN a."Id" END) +
     COUNT(DISTINCT CASE WHEN e."AssessmentId" IS NULL THEN e."Id" END) +
     COUNT(DISTINCT CASE WHEN e."AuditId" IS NULL THEN e."Id" END) +
     COUNT(DISTINCT CASE WHEN c."RiskId" IS NULL THEN c."Id" END)) DESC;


-- ========================================================================================================
-- SECTION 8: REFERENTIAL INTEGRITY VALIDATION
-- ========================================================================================================

-- 8.1 Invalid Risk references (RiskId points to non-existent or deleted Risk)
SELECT
    'Invalid_RiskId_Reference' AS IssueType,
    a."Id",
    a."AssessmentNumber",
    a."RiskId" AS InvalidRiskId,
    a."TenantId"
FROM "Assessments" a
LEFT JOIN "Risks" r ON r."Id" = a."RiskId"
WHERE a."RiskId" IS NOT NULL
  AND a."IsDeleted" = FALSE
  AND (r."Id" IS NULL OR r."IsDeleted" = TRUE);

-- 8.2 Invalid Control references (ControlId points to non-existent or deleted Control)
SELECT
    'Invalid_ControlId_Reference' AS IssueType,
    a."Id",
    a."AssessmentNumber",
    a."ControlId" AS InvalidControlId,
    a."TenantId"
FROM "Assessments" a
LEFT JOIN "Controls" c ON c."Id" = a."ControlId"
WHERE a."ControlId" IS NOT NULL
  AND a."IsDeleted" = FALSE
  AND (c."Id" IS NULL OR c."IsDeleted" = TRUE);

-- 8.3 Invalid Audit references in Evidence (AuditId points to non-existent or deleted Audit)
SELECT
    'Invalid_AuditId_Reference' AS IssueType,
    e."Id",
    e."EvidenceNumber",
    e."AuditId" AS InvalidAuditId,
    e."TenantId"
FROM "Evidences" e
LEFT JOIN "Audits" au ON au."Id" = e."AuditId"
WHERE e."AuditId" IS NOT NULL
  AND e."IsDeleted" = FALSE
  AND (au."Id" IS NULL OR au."IsDeleted" = TRUE);


-- ========================================================================================================
-- SECTION 9: CASCADE DELETE VALIDATION
-- ========================================================================================================

-- 9.1 AuditFindings without valid parent Audit (should be impossible with Cascade)
-- This checks if cascade delete is working correctly
SELECT
    'Orphaned_AuditFinding' AS IssueType,
    af."Id",
    af."FindingNumber",
    af."AuditId" AS InvalidAuditId,
    af."TenantId"
FROM "AuditFindings" af
LEFT JOIN "Audits" a ON a."Id" = af."AuditId"
WHERE af."IsDeleted" = FALSE
  AND (a."Id" IS NULL OR a."IsDeleted" = TRUE);

-- 9.2 PolicyViolations without valid parent Policy (should be impossible with Cascade)
SELECT
    'Orphaned_PolicyViolation' AS IssueType,
    pv."Id",
    pv."ViolationNumber",
    pv."PolicyId" AS InvalidPolicyId,
    pv."TenantId"
FROM "PolicyViolations" pv
LEFT JOIN "Policies" p ON p."Id" = pv."PolicyId"
WHERE pv."IsDeleted" = FALSE
  AND (p."Id" IS NULL OR p."IsDeleted" = TRUE);

-- 9.3 WorkflowExecutions without valid parent Workflow (should be impossible with Cascade)
SELECT
    'Orphaned_WorkflowExecution' AS IssueType,
    we."Id",
    we."ExecutionNumber",
    we."WorkflowId" AS InvalidWorkflowId,
    we."TenantId"
FROM "WorkflowExecutions" we
LEFT JOIN "Workflows" w ON w."Id" = we."WorkflowId"
WHERE we."IsDeleted" = FALSE
  AND (w."Id" IS NULL OR w."IsDeleted" = TRUE);


-- ========================================================================================================
-- SECTION 10: BUSINESS CODE UNIQUENESS VALIDATION
-- ========================================================================================================

-- 10.1 Duplicate BusinessCode in Assessments
SELECT
    'Duplicate_BusinessCode_Assessment' AS IssueType,
    a."BusinessCode",
    COUNT(*) AS DuplicateCount,
    STRING_AGG(a."Id"::TEXT, ', ') AS AffectedIds,
    STRING_AGG(a."AssessmentNumber", ', ') AS AssessmentNumbers
FROM "Assessments" a
WHERE a."BusinessCode" IS NOT NULL
  AND a."IsDeleted" = FALSE
GROUP BY a."BusinessCode"
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- 10.2 Duplicate BusinessCode in Controls
SELECT
    'Duplicate_BusinessCode_Control' AS IssueType,
    c."BusinessCode",
    COUNT(*) AS DuplicateCount,
    STRING_AGG(c."Id"::TEXT, ', ') AS AffectedIds,
    STRING_AGG(c."ControlId", ', ') AS ControlCodes
FROM "Controls" c
WHERE c."BusinessCode" IS NOT NULL
  AND c."IsDeleted" = FALSE
GROUP BY c."BusinessCode"
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- 10.3 Duplicate BusinessCode in Risks
SELECT
    'Duplicate_BusinessCode_Risk' AS IssueType,
    r."BusinessCode",
    COUNT(*) AS DuplicateCount,
    STRING_AGG(r."Id"::TEXT, ', ') AS AffectedIds,
    STRING_AGG(r."Name", ', ') AS RiskNames
FROM "Risks" r
WHERE r."BusinessCode" IS NOT NULL
  AND r."IsDeleted" = FALSE
GROUP BY r."BusinessCode"
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- 10.4 Duplicate BusinessCode in Audits
SELECT
    'Duplicate_BusinessCode_Audit' AS IssueType,
    a."BusinessCode",
    COUNT(*) AS DuplicateCount,
    STRING_AGG(a."Id"::TEXT, ', ') AS AffectedIds,
    STRING_AGG(a."AuditNumber", ', ') AS AuditNumbers
FROM "Audits" a
WHERE a."BusinessCode" IS NOT NULL
  AND a."IsDeleted" = FALSE
GROUP BY a."BusinessCode"
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- 10.5 Duplicate BusinessCode in Evidences
SELECT
    'Duplicate_BusinessCode_Evidence' AS IssueType,
    e."BusinessCode",
    COUNT(*) AS DuplicateCount,
    STRING_AGG(e."Id"::TEXT, ', ') AS AffectedIds,
    STRING_AGG(e."EvidenceNumber", ', ') AS EvidenceNumbers
FROM "Evidences" e
WHERE e."BusinessCode" IS NOT NULL
  AND e."IsDeleted" = FALSE
GROUP BY e."BusinessCode"
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;


-- ========================================================================================================
-- EXECUTION INSTRUCTIONS
-- ========================================================================================================
--
-- 1. Run each section independently to identify specific data integrity issues
-- 2. Focus on sections with non-zero results
-- 3. Priority order:
--    - SECTION 6: Summary Statistics (overview)
--    - SECTION 7: Tenant-level breakdown (identify affected tenants)
--    - SECTION 1-5: Detailed orphaned records
--    - SECTION 8-9: Referential integrity validation
--    - SECTION 10: Business code uniqueness
--
-- 4. Remediation approach:
--    - For orphaned records: Decide whether to re-link to valid parents or soft-delete
--    - For duplicate BusinessCodes: Update one set to use new serial codes
--    - For cascade delete issues: Investigate why cascades didn't trigger
--
-- 5. Database references (GrcDbContext.cs line numbers):
--    - Line 446: Control.RiskId → Risk (SetNull)
--    - Line 462: Assessment.RiskId → Risk (SetNull)
--    - Line 467: Assessment.ControlId → Control (SetNull)
--    - Line 494: AuditFinding.AuditId → Audit (Cascade)
--    - Line 510: Evidence.AssessmentId → Assessment (SetNull)
--    - Line 515: Evidence.AuditId → Audit (SetNull)
--    - Line 520: Evidence.ControlId → Control (SetNull)
--    - Line 525: Evidence.AssessmentRequirementId → AssessmentRequirement (SetNull)
--    - Line 568: PolicyViolation.PolicyId → Policy (Cascade)
--    - Line 595: WorkflowExecution.WorkflowId → Workflow (Cascade)
--
-- ========================================================================================================
