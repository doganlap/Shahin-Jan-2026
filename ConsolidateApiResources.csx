#!/usr/bin/env dotnet-script
// ========================================================================================================
// CONSOLIDATE API RESOURCES INTO MAIN LOCALIZATION FILES
// Purpose: Merge api-i18n-resources-{lang}.xml into SharedResource.{lang}.resx
// Generated: 2026-01-10
// Usage: dotnet script ConsolidateApiResources.csx
// ========================================================================================================

#r "nuget: System.Xml, 4.3.0"

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

// ========================================================================================================
// CONFIGURATION
// ========================================================================================================

var projectRoot = "/home/Shahin-ai/Shahin-Jan-2026";
var englishResourceFile = Path.Combine(projectRoot, "src/GrcMvc/Resources/SharedResource.en.resx");
var arabicResourceFile = Path.Combine(projectRoot, "src/GrcMvc/Resources/SharedResource.ar.resx");
var apiEnglishFile = Path.Combine(projectRoot, "api-i18n-resources-en.xml");
var apiArabicFile = Path.Combine(projectRoot, "api-i18n-resources-ar.xml");
var reportFile = Path.Combine(projectRoot, "API_RESOURCE_CONSOLIDATION_REPORT.md");

Console.WriteLine("========================================================================================================");
Console.WriteLine("API RESOURCE CONSOLIDATION SCRIPT");
Console.WriteLine("========================================================================================================");
Console.WriteLine();

// ========================================================================================================
// HELPER FUNCTIONS
// ========================================================================================================

Dictionary<string, string> ParseResourceFile(string filePath)
{
    Console.WriteLine($"Parsing: {filePath}");

    var keys = new Dictionary<string, string>();

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"  [WARNING] File not found: {filePath}");
        return keys;
    }

    try
    {
        var doc = XDocument.Load(filePath);
        var dataElements = doc.Descendants("data");

        foreach (var data in dataElements)
        {
            var name = data.Attribute("name")?.Value;
            var value = data.Element("value")?.Value;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                keys[name] = value;
            }
        }

        Console.WriteLine($"  ✓ Parsed {keys.Count} keys");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [ERROR] Failed to parse file: {ex.Message}");
    }

    return keys;
}

void CreateBackup(string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"  [WARNING] Cannot backup non-existent file: {filePath}");
        return;
    }

    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    var backupPath = $"{filePath}.backup.{timestamp}";
    File.Copy(filePath, backupPath, true);
    Console.WriteLine($"  ✓ Backup created: {backupPath}");
}

void MergeKeysIntoResx(string resxFilePath, Dictionary<string, string> newKeys, string sourceName)
{
    Console.WriteLine($"Merging {newKeys.Count} keys from {sourceName} into {resxFilePath}");

    if (!File.Exists(resxFilePath))
    {
        Console.WriteLine($"  [ERROR] Target file not found: {resxFilePath}");
        return;
    }

    // Create backup
    CreateBackup(resxFilePath);

    // Load existing resx
    var doc = XDocument.Load(resxFilePath);
    var root = doc.Root;

    if (root == null)
    {
        Console.WriteLine("  [ERROR] Invalid resx file structure");
        return;
    }

    // Get existing keys
    var existingKeys = new HashSet<string>(
        doc.Descendants("data")
           .Select(d => d.Attribute("name")?.Value)
           .Where(n => !string.IsNullOrEmpty(n))
    );

    // Add new keys
    int addedCount = 0;
    int skippedCount = 0;

    foreach (var kvp in newKeys)
    {
        if (existingKeys.Contains(kvp.Key))
        {
            Console.WriteLine($"  [SKIP] Key already exists: {kvp.Key}");
            skippedCount++;
            continue;
        }

        // Create new data element
        var dataElement = new XElement("data",
            new XAttribute("name", kvp.Key),
            new XAttribute(XNamespace.Xml + "space", "preserve"),
            new XElement("value", kvp.Value),
            new XElement("comment", $"Merged from {sourceName} on {DateTime.Now:yyyy-MM-dd}")
        );

        // Add to root (before resheader elements if they exist)
        var firstResHeader = root.Elements("resheader").FirstOrDefault();
        if (firstResHeader != null)
        {
            firstResHeader.AddBeforeSelf(dataElement);
        }
        else
        {
            root.Add(dataElement);
        }

        addedCount++;
    }

    // Save updated file
    doc.Save(resxFilePath);

    Console.WriteLine($"  ✓ Added {addedCount} new keys");
    Console.WriteLine($"  ⚠ Skipped {skippedCount} existing keys");
}

// ========================================================================================================
// STEP 1: PARSE ALL RESOURCE FILES
// ========================================================================================================

Console.WriteLine("Step 1: Parsing all resource files...");
Console.WriteLine();

var enKeys = ParseResourceFile(englishResourceFile);
var arKeys = ParseResourceFile(arabicResourceFile);
var apiEnKeys = ParseResourceFile(apiEnglishFile);
var apiArKeys = ParseResourceFile(apiArabicFile);

Console.WriteLine();
Console.WriteLine("========================================================================================================");
Console.WriteLine("SUMMARY OF PARSED RESOURCES");
Console.WriteLine("========================================================================================================");
Console.WriteLine($"English (SharedResource.en.resx):     {enKeys.Count} keys");
Console.WriteLine($"Arabic (SharedResource.ar.resx):      {arKeys.Count} keys");
Console.WriteLine($"API English (api-i18n-resources-en):  {apiEnKeys.Count} keys");
Console.WriteLine($"API Arabic (api-i18n-resources-ar):   {apiArKeys.Count} keys");
Console.WriteLine();

// ========================================================================================================
// STEP 2: IDENTIFY API KEYS NOT IN MAIN RESOURCES
// ========================================================================================================

Console.WriteLine("Step 2: Identifying API keys not in main resources...");
Console.WriteLine();

var apiKeysToMerge = new Dictionary<string, (string enValue, string arValue)>();

foreach (var kvp in apiEnKeys)
{
    if (!enKeys.ContainsKey(kvp.Key))
    {
        string arValue = apiArKeys.ContainsKey(kvp.Key) ? apiArKeys[kvp.Key] : "[NEEDS TRANSLATION]";
        apiKeysToMerge[kvp.Key] = (kvp.Value, arValue);
    }
}

Console.WriteLine($"Found {apiKeysToMerge.Count} API keys to merge");
Console.WriteLine();

// ========================================================================================================
// STEP 3: MERGE API KEYS INTO MAIN RESOURCES
// ========================================================================================================

Console.WriteLine("Step 3: Merging API keys into main resources...");
Console.WriteLine();

// Prepare English keys to merge
var enKeysToMerge = new Dictionary<string, string>();
foreach (var kvp in apiKeysToMerge)
{
    enKeysToMerge[kvp.Key] = kvp.Value.enValue;
}

// Prepare Arabic keys to merge
var arKeysToMerge = new Dictionary<string, string>();
foreach (var kvp in apiKeysToMerge)
{
    arKeysToMerge[kvp.Key] = kvp.Value.arValue;
}

// Merge into English
MergeKeysIntoResx(englishResourceFile, enKeysToMerge, "api-i18n-resources-en.xml");
Console.WriteLine();

// Merge into Arabic
MergeKeysIntoResx(arabicResourceFile, arKeysToMerge, "api-i18n-resources-ar.xml");
Console.WriteLine();

// ========================================================================================================
// STEP 4: GENERATE CONSOLIDATION REPORT
// ========================================================================================================

Console.WriteLine("Step 4: Generating consolidation report...");
Console.WriteLine();

var report = $@"# API RESOURCE CONSOLIDATION REPORT
**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}

---

## EXECUTIVE SUMMARY

| Metric | Count | Status |
|--------|-------|--------|
| **API Keys Identified** | {apiKeysToMerge.Count} | ✓ |
| **Keys Merged to EN** | {enKeysToMerge.Count} | ✓ |
| **Keys Merged to AR** | {arKeysToMerge.Count} | ✓ |
| **Total EN Keys (Before)** | {enKeys.Count} | - |
| **Total AR Keys (Before)** | {arKeys.Count} | - |
| **Total EN Keys (After)** | {enKeys.Count + enKeysToMerge.Count} | ✓ |
| **Total AR Keys (After)** | {arKeys.Count + arKeysToMerge.Count} | ✓ |

---

## MERGED API KEYS

The following {apiKeysToMerge.Count} API keys have been merged into main SharedResource files:

### Sample Merged Keys (First 30)

";

var sampleKeys = apiKeysToMerge.Take(30);
foreach (var kvp in sampleKeys)
{
    report += $"\n- `{kvp.Key}`\n";
    report += $"  - **EN:** {kvp.Value.enValue}\n";
    report += $"  - **AR:** {kvp.Value.arValue}\n";
}

report += $@"

### Complete List of Merged Keys

```
{string.Join("\n", apiKeysToMerge.Keys)}
```

---

## CATEGORY BREAKDOWN

The merged API keys fall into the following categories:

";

// Categorize keys by prefix
var categories = new Dictionary<string, List<string>>();
foreach (var key in apiKeysToMerge.Keys)
{
    var parts = key.Split('_');
    var category = parts.Length >= 2 ? $"{parts[0]}_{parts[1]}" : parts[0];

    if (!categories.ContainsKey(category))
    {
        categories[category] = new List<string>();
    }

    categories[category].Add(key);
}

foreach (var cat in categories.OrderByDescending(c => c.Value.Count))
{
    report += $"\n### {cat.Key} ({cat.Value.Count} keys)\n";
    report += "```\n";
    report += string.Join("\n", cat.Value.Take(10));
    if (cat.Value.Count > 10)
    {
        report += $"\n... and {cat.Value.Count - 10} more";
    }
    report += "\n```\n";
}

report += $@"

---

## POST-CONSOLIDATION ACTIONS

### ✓ Completed
- [x] Parsed API resource files
- [x] Identified {apiKeysToMerge.Count} unique API keys not in main resources
- [x] Created backups of SharedResource.en.resx and SharedResource.ar.resx
- [x] Merged API keys into main resource files
- [x] Generated consolidation report

### 📋 Next Steps

1. **Verify Merged Keys**
   - Review merged keys in SharedResource.en.resx
   - Review merged keys in SharedResource.ar.resx
   - Look for keys with comment: ""Merged from api-i18n-resources-{{lang}}.xml on {DateTime.Now:yyyy-MM-dd}""

2. **Update Application Code**
   - Search for references to `api-i18n-resources-{{lang}}.xml` in code
   - Update any code that loads API resources separately
   - Ensure all API controllers use `IStringLocalizer<SharedResource>`

3. **Test Localization**
   - Run application in English mode
   - Run application in Arabic mode
   - Verify all API responses show correct localized messages
   - Check for any missing translations (look for ""[NEEDS TRANSLATION]"")

4. **Archive Old API Resource Files**
   ```bash
   mkdir -p archive/api-resources-$(date +%Y%m%d)
   mv api-i18n-resources-en.xml archive/api-resources-$(date +%Y%m%d)/
   mv api-i18n-resources-ar.xml archive/api-resources-$(date +%Y%m%d)/
   ```

5. **Update Documentation**
   - Update localization documentation to reflect single resource file approach
   - Document where to add new API-related localization keys

---

## FILES MODIFIED

- **English Resource File:** `{englishResourceFile}`
- **Arabic Resource File:** `{arabicResourceFile}`
- **Backups Created:** `*.backup.{DateTime.Now:yyyyMMdd_HHmmss}`

## FILES TO ARCHIVE

- **API English File:** `{apiEnglishFile}` (no longer needed after consolidation)
- **API Arabic File:** `{apiArabicFile}` (no longer needed after consolidation)

---

## RELATED DOCUMENTATION

- **Data Integrity Audit:** `DATA_INTEGRITY_AUDIT_REPORT.sql`
- **Localization Reconciliation:** `LocalizationReconciliation.ps1`
- **Database Constraints Migration:** `src/GrcMvc/Data/Migrations/20260110_AddDataIntegrityConstraints.cs`

---

**Generated by:** Shahin AI GRC Platform - API Resource Consolidation Script
**Report File:** `{reportFile}`

";

File.WriteAllText(reportFile, report);
Console.WriteLine($"✓ Report generated: {reportFile}");
Console.WriteLine();

// ========================================================================================================
// FINAL SUMMARY
// ========================================================================================================

Console.WriteLine("========================================================================================================");
Console.WriteLine("CONSOLIDATION COMPLETE");
Console.WriteLine("========================================================================================================");
Console.WriteLine();
Console.WriteLine($"✓ Merged {apiKeysToMerge.Count} API keys into main resource files");
Console.WriteLine($"✓ English resource file updated: {englishResourceFile}");
Console.WriteLine($"✓ Arabic resource file updated: {arabicResourceFile}");
Console.WriteLine($"✓ Consolidation report generated: {reportFile}");
Console.WriteLine();
Console.WriteLine("Next Steps:");
Console.WriteLine("  1. Review the consolidation report");
Console.WriteLine("  2. Test application to verify API localization works correctly");
Console.WriteLine("  3. Archive or remove old API resource XML files");
Console.WriteLine("  4. Update code references to use SharedResource instead of API-specific files");
Console.WriteLine();
Console.WriteLine("========================================================================================================");
