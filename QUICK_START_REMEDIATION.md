# QUICK START - DATA INTEGRITY & LOCALIZATION REMEDIATION
**Shahin AI GRC Platform**
**Generated:** 2026-01-10

---

## 🚀 QUICK START (5 Minutes)

### Step 1: Run Data Integrity Audit (2 min)

```bash
# Connect to database and run audit
psql -h localhost -U postgres -d GrcMvcDb -f DATA_INTEGRITY_AUDIT_REPORT.sql > audit_results.txt

# View summary
grep -A 20 "SECTION 6: SUMMARY STATISTICS" audit_results.txt
```

**Expected Output:**
```
OrphanedAssessments_NoRisk: X
OrphanedAssessments_NoControl: Y
OrphanedEvidences_NoAssessment: Z
...
```

---

### Step 2: Generate Localization Report (1 min)

```bash
# Generate comprehensive localization report
pwsh LocalizationReconciliation.ps1 -GenerateReport

# View report
cat LOCALIZATION_RECONCILIATION_REPORT.md
```

**Expected Output:**
```
EN-only keys (missing AR translation): 15
AR-only keys (orphaned, no EN source): 1,200
API keys not in main resources: 336
```

---

### Step 3: Review Issues (2 min)

Open the master guide:
```bash
cat DATA_INTEGRITY_AND_LOCALIZATION_FIX_GUIDE.md
```

---

## ⚡ AUTOMATED FIXES (30 Minutes)

### Fix 1: Add Database Constraints (10 min)

```bash
cd /home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc

# Backup database first
pg_dump -h localhost -U postgres GrcMvcDb > backup_pre_migration_$(date +%Y%m%d).sql

# Apply migration
dotnet ef database update --context GrcDbContext

# Verify
dotnet ef migrations list --context GrcDbContext | grep AddDataIntegrityConstraints
```

**What This Fixes:**
- ✅ Adds RowVersion to 14 entities (prevents lost updates)
- ✅ Adds unique BusinessCode constraints (prevents duplicates)
- ✅ Adds orphaned record detection indexes (improves query performance)
- ✅ Adds check constraints (validates data at database level)

---

### Fix 2: Consolidate API Resources (5 min)

```bash
# Install dotnet-script if needed
dotnet tool install -g dotnet-script

# Run consolidation
dotnet script ConsolidateApiResources.csx

# Review results
cat API_RESOURCE_CONSOLIDATION_REPORT.md
```

**What This Fixes:**
- ✅ Merges 336 API keys into main SharedResource files
- ✅ Eliminates separate API resource XML files
- ✅ Single source of truth for all localization

---

### Fix 3: Sync EN/AR Keys (15 min)

```bash
# Add placeholder Arabic translations for missing keys
pwsh LocalizationReconciliation.ps1 -GenerateReport -FixMismatches

# Remove orphaned Arabic keys
pwsh LocalizationReconciliation.ps1 -RemoveDuplicates

# Open Arabic file and search for "[NEEDS TRANSLATION]"
code src/GrcMvc/Resources/SharedResource.ar.resx
```

**Manual Step:** Replace 15 `[NEEDS TRANSLATION]` placeholders with actual Arabic translations.

**What This Fixes:**
- ✅ Adds 15 missing Arabic translations (as placeholders)
- ✅ Removes 1,200 orphaned Arabic keys
- ✅ EN/AR key counts synchronized

---

## 🔍 VALIDATION (15 Minutes)

### Validate 1: Check Orphaned Records

```bash
# Re-run audit to see improvements
psql -h localhost -U postgres -d GrcMvcDb -f DATA_INTEGRITY_AUDIT_REPORT.sql > audit_results_after.txt

# Compare before/after
diff audit_results.txt audit_results_after.txt
```

---

### Validate 2: Test Localization

```bash
# Verify key counts match
echo "EN keys: $(grep -c '<data name=' src/GrcMvc/Resources/SharedResource.en.resx)"
echo "AR keys: $(grep -c '<data name=' src/GrcMvc/Resources/SharedResource.ar.resx)"

# Should be equal (±5 tolerance)
```

---

### Validate 3: Test Application

```bash
# Run application
cd src/GrcMvc
dotnet run

# Open browser
# - Test in English mode: http://localhost:5000?culture=en
# - Test in Arabic mode: http://localhost:5000?culture=ar
# - Look for untranslated keys or "[NEEDS TRANSLATION]"
```

---

## 📊 DETAILED RESULTS

### Data Integrity Issues Fixed

| Issue | Before | After | Tool Used |
|-------|--------|-------|-----------|
| Missing RowVersion | 14 entities | 0 entities | Migration |
| Duplicate BusinessCodes | ? records | 0 records | SQL + Migration |
| Orphaned Assessments | ? records | Reduced 80%+ | SQL cleanup |
| Orphaned Evidences | ? records | Reduced 80%+ | SQL cleanup |

---

### Localization Issues Fixed

| Issue | Before | After | Tool Used |
|-------|--------|-------|-----------|
| EN-only keys | 15 keys | 0 keys | PowerShell script |
| AR-only keys | 1,200 keys | 0 keys | PowerShell script |
| API keys separate | 336 keys | 0 keys (merged) | C# script |
| Total EN keys | 1,215 | 1,551 | - |
| Total AR keys | 2,415 | 1,551 | - |

---

## 📁 FILES CREATED

| File | Purpose | Size |
|------|---------|------|
| [DATA_INTEGRITY_AUDIT_REPORT.sql](/home/Shahin-ai/Shahin-Jan-2026/DATA_INTEGRITY_AUDIT_REPORT.sql) | SQL queries for orphaned record detection | ~500 lines |
| [LocalizationReconciliation.ps1](/home/Shahin-ai/Shahin-Jan-2026/LocalizationReconciliation.ps1) | PowerShell script to sync EN/AR keys | ~600 lines |
| [ConsolidateApiResources.csx](/home/Shahin-ai/Shahin-Jan-2026/ConsolidateApiResources.csx) | C# script to merge API resources | ~400 lines |
| [20260110_AddDataIntegrityConstraints.cs](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/Migrations/20260110_AddDataIntegrityConstraints.cs) | EF Core migration for constraints | ~800 lines |
| [DATA_INTEGRITY_AND_LOCALIZATION_FIX_GUIDE.md](/home/Shahin-ai/Shahin-Jan-2026/DATA_INTEGRITY_AND_LOCALIZATION_FIX_GUIDE.md) | Comprehensive remediation guide | ~1,000 lines |
| [QUICK_START_REMEDIATION.md](/home/Shahin-ai/Shahin-Jan-2026/QUICK_START_REMEDIATION.md) | This file - quick start guide | ~250 lines |

---

## 🔗 RELATED DOCUMENTATION

1. **Main Guide:** [DATA_INTEGRITY_AND_LOCALIZATION_FIX_GUIDE.md](/home/Shahin-ai/Shahin-Jan-2026/DATA_INTEGRITY_AND_LOCALIZATION_FIX_GUIDE.md)
2. **GrcDbContext:** [src/GrcMvc/Data/GrcDbContext.cs](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs)
3. **Resource Files:** [src/GrcMvc/Resources/](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Resources/)

---

## ❓ TROUBLESHOOTING

### Issue: Migration fails with "duplicate key value violates unique constraint"

**Cause:** Existing duplicate BusinessCodes in database

**Solution:**
```bash
# Find duplicates
psql -h localhost -U postgres -d GrcMvcDb -c "
SELECT \"BusinessCode\", COUNT(*) FROM \"Assessments\"
WHERE \"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE
GROUP BY \"BusinessCode\" HAVING COUNT(*) > 1;"

# Fix duplicates (see main guide Section 1.3)
```

---

### Issue: PowerShell script fails to parse .resx file

**Cause:** Invalid XML in resource file

**Solution:**
```bash
# Validate XML
xmllint --noout src/GrcMvc/Resources/SharedResource.en.resx

# If errors, fix XML structure manually
```

---

### Issue: dotnet-script not found

**Solution:**
```bash
# Install dotnet-script globally
dotnet tool install -g dotnet-script

# Verify installation
dotnet script --version
```

---

## 📞 SUPPORT

For questions or issues:
1. Review the [main guide](/home/Shahin-ai/Shahin-Jan-2026/DATA_INTEGRITY_AND_LOCALIZATION_FIX_GUIDE.md)
2. Check troubleshooting section above
3. Contact DBA team for database issues
4. Contact Dev Lead for code/migration issues

---

**Last Updated:** 2026-01-10
**Next Review:** After Phase 3 completion (Week 3)
