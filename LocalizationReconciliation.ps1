# ========================================================================================================
# LOCALIZATION RECONCILIATION SCRIPT
# Purpose: Sync English and Arabic resource keys, identify orphans, and merge API resources
# Generated: 2026-01-10
# ========================================================================================================

param(
    [string]$ProjectRoot = "/home/Shahin-ai/Shahin-Jan-2026",
    [switch]$GenerateReport,
    [switch]$FixMismatches,
    [switch]$MergeApiResources,
    [switch]$RemoveDuplicates
)

# Color output functions
function Write-Success { param([string]$Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param([string]$Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host $Message -ForegroundColor Red }
function Write-Info { param([string]$Message) Write-Host $Message -ForegroundColor Cyan }

# File paths
$EnglishResourceFile = "$ProjectRoot/src/GrcMvc/Resources/SharedResource.en.resx"
$ArabicResourceFile = "$ProjectRoot/src/GrcMvc/Resources/SharedResource.ar.resx"
$BaseResourceFile = "$ProjectRoot/src/GrcMvc/Resources/SharedResource.resx"
$ApiEnglishFile = "$ProjectRoot/api-i18n-resources-en.xml"
$ApiArabicFile = "$ProjectRoot/api-i18n-resources-ar.xml"
$ReportFile = "$ProjectRoot/LOCALIZATION_RECONCILIATION_REPORT.md"

Write-Info "========================================================================================================="
Write-Info "LOCALIZATION RECONCILIATION SCRIPT"
Write-Info "========================================================================================================="
Write-Info ""

# ========================================================================================================
# STEP 1: Parse Resource Files
# ========================================================================================================

function Parse-ResxFile {
    param([string]$FilePath)

    Write-Info "Parsing resource file: $FilePath"

    [xml]$xml = Get-Content $FilePath
    $keys = @{}

    foreach ($data in $xml.root.data) {
        $keyName = $data.name
        $value = $data.value

        if ($keyName -and $value) {
            $keys[$keyName] = $value
        }
    }

    Write-Success "  ✓ Parsed $($keys.Count) keys"
    return $keys
}

# Parse main resource files
Write-Info "`nStep 1: Parsing main resource files..."
$enKeys = Parse-ResxFile -FilePath $EnglishResourceFile
$arKeys = Parse-ResxFile -FilePath $ArabicResourceFile
$baseKeys = Parse-ResxFile -FilePath $BaseResourceFile

# Parse API resource files
Write-Info "`nStep 2: Parsing API resource files..."
$apiEnKeys = Parse-ResxFile -FilePath $ApiEnglishFile
$apiArKeys = Parse-ResxFile -FilePath $ApiArabicFile

Write-Info ""
Write-Info "========================================================================================================="
Write-Info "SUMMARY OF PARSED RESOURCES"
Write-Info "========================================================================================================="
Write-Success "English (SharedResource.en.resx):     $($enKeys.Count) keys"
Write-Success "Arabic (SharedResource.ar.resx):      $($arKeys.Count) keys"
Write-Success "Base (SharedResource.resx):           $($baseKeys.Count) keys"
Write-Success "API English (api-i18n-resources-en):  $($apiEnKeys.Count) keys"
Write-Success "API Arabic (api-i18n-resources-ar):   $($apiArKeys.Count) keys"
Write-Info ""

# ========================================================================================================
# STEP 3: Identify Mismatches
# ========================================================================================================

Write-Info "========================================================================================================="
Write-Info "ANALYZING MISMATCHES"
Write-Info "========================================================================================================="
Write-Info ""

# Keys in EN but not in AR
$enOnlyKeys = @()
foreach ($key in $enKeys.Keys) {
    if (-not $arKeys.ContainsKey($key)) {
        $enOnlyKeys += $key
    }
}

# Keys in AR but not in EN
$arOnlyKeys = @()
foreach ($key in $arKeys.Keys) {
    if (-not $enKeys.ContainsKey($key)) {
        $arOnlyKeys += $key
    }
}

# Common keys (exists in both)
$commonKeys = @()
foreach ($key in $enKeys.Keys) {
    if ($arKeys.ContainsKey($key)) {
        $commonKeys += $key
    }
}

# API keys not in main resources
$apiKeysNotInMain = @()
foreach ($key in $apiEnKeys.Keys) {
    if (-not $enKeys.ContainsKey($key)) {
        $apiKeysNotInMain += $key
    }
}

Write-Warning "EN-only keys (missing AR translation): $($enOnlyKeys.Count)"
Write-Warning "AR-only keys (orphaned, no EN source): $($arOnlyKeys.Count)"
Write-Success "Common keys (in both EN and AR):       $($commonKeys.Count)"
Write-Warning "API keys not in main resources:        $($apiKeysNotInMain.Count)"
Write-Info ""

# ========================================================================================================
# STEP 4: Generate Detailed Report
# ========================================================================================================

if ($GenerateReport) {
    Write-Info "Generating detailed reconciliation report..."

    $report = @"
# LOCALIZATION RECONCILIATION REPORT
**Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

---

## EXECUTIVE SUMMARY

| Metric | Count | Status |
|--------|-------|--------|
| **Total EN Keys** | $($enKeys.Count) | ✓ |
| **Total AR Keys** | $($arKeys.Count) | ⚠ $(if ($arKeys.Count -gt $enKeys.Count) { "+$($arKeys.Count - $enKeys.Count) extra" } else { "" }) |
| **Common Keys** | $($commonKeys.Count) | ✓ |
| **EN-only Keys** | $($enOnlyKeys.Count) | ⚠ Missing AR translations |
| **AR-only Keys** | $($arOnlyKeys.Count) | ⚠ Orphaned AR keys |
| **API EN Keys** | $($apiEnKeys.Count) | ℹ Separate file |
| **API AR Keys** | $($apiArKeys.Count) | ℹ Separate file |
| **API Keys Not in Main** | $($apiKeysNotInMain.Count) | ⚠ Needs consolidation |

---

## ISSUE 1: ENGLISH KEYS MISSING ARABIC TRANSLATION

**Count:** $($enOnlyKeys.Count) keys

### Critical EN-only Keys (Sample - First 20)

"@

    $enOnlyKeys | Select-Object -First 20 | ForEach-Object {
        $report += "`n- ``$_`` → **EN:** $($enKeys[$_])"
    }

    $report += @"


### Complete List of EN-only Keys

``````
$($enOnlyKeys -join "`n")
``````

---

## ISSUE 2: ORPHANED ARABIC KEYS (NO ENGLISH SOURCE)

**Count:** $($arOnlyKeys.Count) keys

These keys exist in Arabic but have no corresponding English source. Likely duplicates or variants.

### Sample Orphaned AR Keys (First 20)

"@

    $arOnlyKeys | Select-Object -First 20 | ForEach-Object {
        $report += "`n- ``$_`` → **AR:** $($arKeys[$_])"
    }

    $report += @"


### Complete List of Orphaned AR Keys

``````
$($arOnlyKeys -join "`n")
``````

---

## ISSUE 3: API RESOURCE KEYS NOT IN MAIN RESOURCES

**Count:** $($apiKeysNotInMain.Count) keys

These API-specific keys are in separate files and should be consolidated into main SharedResource files.

### API Keys to Merge (Sample - First 30)

"@

    $apiKeysNotInMain | Select-Object -First 30 | ForEach-Object {
        $report += "`n- ``$_`` → **EN:** $($apiEnKeys[$_]) | **AR:** $($apiArKeys[$_])"
    }

    $report += @"


### Complete List of API Keys to Merge

``````
$($apiKeysNotInMain -join "`n")
``````

---

## RECOMMENDED ACTIONS

### Priority 1: Add Missing Arabic Translations
- **Action:** Translate $($enOnlyKeys.Count) EN-only keys to Arabic
- **Command:** Run script with ``-FixMismatches`` flag (creates placeholder AR entries)
- **Manual Step:** Replace placeholders with actual Arabic translations

### Priority 2: Remove Orphaned Arabic Keys
- **Action:** Remove $($arOnlyKeys.Count) AR-only keys that have no English source
- **Command:** Run script with ``-RemoveDuplicates`` flag
- **Risk:** Low (these are likely duplicates/variants)

### Priority 3: Consolidate API Resources
- **Action:** Merge $($apiKeysNotInMain.Count) API keys into main SharedResource files
- **Command:** Run script with ``-MergeApiResources`` flag
- **Benefit:** Single source of truth for all localization keys

---

## TECHNICAL DETAILS

### DeleteBehavior.SetNull Relationships (Data Integrity Context)

The following relationships use SetNull behavior (from GrcDbContext.cs):

| Entity | Foreign Key | Parent | Line | Impact |
|--------|-------------|--------|------|--------|
| Control | RiskId | Risk | 446 | Controls orphaned when Risk deleted |
| Assessment | RiskId | Risk | 462 | Assessments orphaned when Risk deleted |
| Assessment | ControlId | Control | 467 | Assessments orphaned when Control deleted |
| Evidence | AssessmentId | Assessment | 510 | Evidence orphaned when Assessment deleted |
| Evidence | AuditId | Audit | 515 | Evidence orphaned when Audit deleted |
| Evidence | ControlId | Control | 520 | Evidence orphaned when Control deleted |
| Evidence | AssessmentRequirementId | AssessmentRequirement | 525 | Evidence orphaned when Requirement deleted |
| ControlImplementation | ControlId | Control | 1206 | Implementation orphaned when Control deleted |
| Baseline | FrameworkId | Framework | 1236 | Baseline orphaned when Framework deleted |

---

## FILES ANALYZED

- **English:** ``$EnglishResourceFile``
- **Arabic:** ``$ArabicResourceFile``
- **Base:** ``$BaseResourceFile``
- **API EN:** ``$ApiEnglishFile``
- **API AR:** ``$ApiArabicFile``

---

**Generated by:** Shahin AI GRC Platform - Localization Reconciliation Script
**Report File:** ``$ReportFile``

"@

    $report | Out-File -FilePath $ReportFile -Encoding UTF8
    Write-Success "✓ Report generated: $ReportFile"
    Write-Info ""
}

# ========================================================================================================
# STEP 5: Fix Mismatches (Add Missing AR Keys)
# ========================================================================================================

if ($FixMismatches) {
    Write-Info "========================================================================================================="
    Write-Info "FIXING MISMATCHES - ADDING MISSING ARABIC TRANSLATIONS"
    Write-Info "========================================================================================================="
    Write-Info ""

    Write-Warning "This will add $($enOnlyKeys.Count) new keys to Arabic resource file with placeholder translations."
    Write-Warning "You will need to manually translate these keys after running this script."
    Write-Info ""

    # Backup Arabic file
    $backupFile = "$ArabicResourceFile.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $ArabicResourceFile $backupFile
    Write-Success "✓ Backup created: $backupFile"

    # Load Arabic XML
    [xml]$arXml = Get-Content $ArabicResourceFile

    # Add missing keys
    $addedCount = 0
    foreach ($key in $enOnlyKeys) {
        $newData = $arXml.CreateElement("data")
        $newData.SetAttribute("name", $key)
        $newData.SetAttribute("xml:space", "preserve")

        $newValue = $arXml.CreateElement("value")
        $newValue.InnerText = "[NEEDS TRANSLATION] $($enKeys[$key])"
        $newData.AppendChild($newValue) | Out-Null

        $arXml.root.AppendChild($newData) | Out-Null
        $addedCount++
    }

    # Save updated Arabic file
    $arXml.Save($ArabicResourceFile)
    Write-Success "✓ Added $addedCount placeholder translations to Arabic resource file"
    Write-Warning "⚠ IMPORTANT: Search for '[NEEDS TRANSLATION]' in $ArabicResourceFile and replace with actual Arabic translations"
    Write-Info ""
}

# ========================================================================================================
# STEP 6: Remove Duplicate/Orphaned AR Keys
# ========================================================================================================

if ($RemoveDuplicates) {
    Write-Info "========================================================================================================="
    Write-Info "REMOVING ORPHANED ARABIC KEYS"
    Write-Info "========================================================================================================="
    Write-Info ""

    Write-Warning "This will remove $($arOnlyKeys.Count) orphaned keys from Arabic resource file."
    Write-Info ""

    # Backup Arabic file
    $backupFile = "$ArabicResourceFile.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $ArabicResourceFile $backupFile
    Write-Success "✓ Backup created: $backupFile"

    # Load Arabic XML
    [xml]$arXml = Get-Content $ArabicResourceFile

    # Remove orphaned keys
    $removedCount = 0
    foreach ($key in $arOnlyKeys) {
        $nodeToRemove = $arXml.root.data | Where-Object { $_.name -eq $key }
        if ($nodeToRemove) {
            $arXml.root.RemoveChild($nodeToRemove) | Out-Null
            $removedCount++
        }
    }

    # Save cleaned Arabic file
    $arXml.Save($ArabicResourceFile)
    Write-Success "✓ Removed $removedCount orphaned keys from Arabic resource file"
    Write-Info ""
}

# ========================================================================================================
# STEP 7: Merge API Resources into Main Resources
# ========================================================================================================

if ($MergeApiResources) {
    Write-Info "========================================================================================================="
    Write-Info "MERGING API RESOURCES INTO MAIN RESOURCES"
    Write-Info "========================================================================================================="
    Write-Info ""

    Write-Warning "This will merge $($apiKeysNotInMain.Count) API keys into main resource files."
    Write-Info ""

    # Backup main files
    $enBackup = "$EnglishResourceFile.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    $arBackup = "$ArabicResourceFile.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $EnglishResourceFile $enBackup
    Copy-Item $ArabicResourceFile $arBackup
    Write-Success "✓ Backups created"

    # Load main XML files
    [xml]$enXml = Get-Content $EnglishResourceFile
    [xml]$arXml = Get-Content $ArabicResourceFile

    # Add API keys to English
    $addedEnCount = 0
    foreach ($key in $apiKeysNotInMain) {
        $newData = $enXml.CreateElement("data")
        $newData.SetAttribute("name", $key)
        $newData.SetAttribute("xml:space", "preserve")

        $newValue = $enXml.CreateElement("value")
        $newValue.InnerText = $apiEnKeys[$key]
        $newData.AppendChild($newValue) | Out-Null

        # Add comment indicating source
        $newComment = $enXml.CreateElement("comment")
        $newComment.InnerText = "Merged from API resources on $(Get-Date -Format 'yyyy-MM-dd')"
        $newData.AppendChild($newComment) | Out-Null

        $enXml.root.AppendChild($newData) | Out-Null
        $addedEnCount++
    }

    # Add API keys to Arabic
    $addedArCount = 0
    foreach ($key in $apiKeysNotInMain) {
        if ($apiArKeys.ContainsKey($key)) {
            $newData = $arXml.CreateElement("data")
            $newData.SetAttribute("name", $key)
            $newData.SetAttribute("xml:space", "preserve")

            $newValue = $arXml.CreateElement("value")
            $newValue.InnerText = $apiArKeys[$key]
            $newData.AppendChild($newValue) | Out-Null

            # Add comment indicating source
            $newComment = $arXml.CreateElement("comment")
            $newComment.InnerText = "Merged from API resources on $(Get-Date -Format 'yyyy-MM-dd')"
            $newData.AppendChild($newComment) | Out-Null

            $arXml.root.AppendChild($newData) | Out-Null
            $addedArCount++
        }
    }

    # Save updated files
    $enXml.Save($EnglishResourceFile)
    $arXml.Save($ArabicResourceFile)

    Write-Success "✓ Added $addedEnCount keys to English resource file"
    Write-Success "✓ Added $addedArCount keys to Arabic resource file"
    Write-Info ""
    Write-Warning "⚠ Consider archiving or removing the API resource files after verification:"
    Write-Info "  - $ApiEnglishFile"
    Write-Info "  - $ApiArabicFile"
    Write-Info ""
}

# ========================================================================================================
# FINAL SUMMARY
# ========================================================================================================

Write-Info "========================================================================================================="
Write-Info "SCRIPT EXECUTION COMPLETE"
Write-Info "========================================================================================================="
Write-Info ""
Write-Success "Tasks completed:"
if ($GenerateReport) { Write-Success "  ✓ Generated reconciliation report: $ReportFile" }
if ($FixMismatches) { Write-Success "  ✓ Added $($enOnlyKeys.Count) placeholder translations to Arabic" }
if ($RemoveDuplicates) { Write-Success "  ✓ Removed $($arOnlyKeys.Count) orphaned Arabic keys" }
if ($MergeApiResources) { Write-Success "  ✓ Merged $($apiKeysNotInMain.Count) API keys into main resources" }

Write-Info ""
Write-Info "Next steps:"
Write-Info "  1. Review generated report: $ReportFile"
if ($FixMismatches) {
    Write-Info "  2. Search for '[NEEDS TRANSLATION]' in Arabic file and translate"
}
if ($MergeApiResources) {
    Write-Info "  3. Test application to ensure all API responses use correct localization"
    Write-Info "  4. Archive/remove old API resource XML files after verification"
}
Write-Info ""

# ========================================================================================================
# USAGE EXAMPLES
# ========================================================================================================
#
# 1. Generate report only (no changes):
#    pwsh LocalizationReconciliation.ps1 -GenerateReport
#
# 2. Generate report + add missing AR translations:
#    pwsh LocalizationReconciliation.ps1 -GenerateReport -FixMismatches
#
# 3. Generate report + remove orphaned AR keys:
#    pwsh LocalizationReconciliation.ps1 -GenerateReport -RemoveDuplicates
#
# 4. Generate report + merge API resources:
#    pwsh LocalizationReconciliation.ps1 -GenerateReport -MergeApiResources
#
# 5. Full reconciliation (all operations):
#    pwsh LocalizationReconciliation.ps1 -GenerateReport -FixMismatches -RemoveDuplicates -MergeApiResources
#
# ========================================================================================================
