# DTO Alias Consistency Review

## File Naming Clarification

**File:** `src/GrcMvc/Controllers/CCMController.cs`

This file contains multiple controller classes:
- `CCMController` (lines 15-76)
- `KRIDashboardController` (lines 77-130)
- `ExceptionController` (lines 131-223)
- `AuditPackageController` (lines 224-291) ← **Logger added here**
- `InvitationController` (lines 300-349)
- `ReportsController` (lines 350+)

The logger was correctly added to `AuditPackageController` class within `CCMController.cs`. This is a multi-class file pattern, which is acceptable but worth noting for maintainability.

---

## 1. Serialization Safety Review

### ✅ All Aliases Properly Tagged

**System.Text.Json** (ASP.NET Core default):
- All alias properties use `[System.Text.Json.Serialization.JsonIgnore]`

**Newtonsoft.Json** (used in WorkflowService):
- All alias properties use `[Newtonsoft.Json.JsonIgnore]`

**Dual-tagging verified for:**
- `AuditDto`: `Title`, `AuditType`, `ScheduledDate`, `AuditorId`
- `ControlDto`: `ControlNumber`, `ControlType`, `TestingFrequency`, `LastTestedDate`
- `RiskDto` (all 3 classes): `RiskScore`, `TreatmentPlan`
- `AssessmentDto`: `AssessmentType`, `CompletedDate`, `AssessorId`, `Notes`

**API Contract Stability:**
✅ Canonical fields (`Name`, `Type`, `ControlId`, `ResidualRisk`, `MitigationStrategy`, etc.) will appear in JSON responses
✅ Alias fields will **not** appear in JSON responses
✅ No duplicate keys in serialized output

---

## 2. AutoMapper Profile Correctness

### ✅ AutoMapper Uses Canonical Fields Only

**Verified mappings:**

```csharp
// Audit mappings
CreateMap<Audit, AuditDto>()              // Maps to Name, Type, PlannedStartDate, LeadAuditor (canonical)
CreateMap<CreateAuditDto, Audit>()        // Maps from canonical fields
CreateMap<UpdateAuditDto, Audit>()        // Maps from canonical fields

// Control mappings
CreateMap<Control, ControlDto>()          // Maps to ControlId, Type, Frequency, LastTestDate (canonical)
CreateMap<CreateControlDto, Control>()    // Maps from canonical fields
CreateMap<UpdateControlDto, Control>()    // Maps from canonical fields

// Risk mappings
CreateMap<Risk, RiskDto>()                // Maps to ResidualRisk, MitigationStrategy (canonical)
CreateMap<CreateRiskDto, Risk>()          // Maps from canonical fields
CreateMap<UpdateRiskDto, Risk>()          // Maps from canonical fields

// Assessment mappings
CreateMap<Assessment, AssessmentDto>()    // Maps to Type, AssignedTo, EndDate (canonical)
CreateMap<CreateAssessmentDto, Assessment>()  // Maps from canonical fields
CreateMap<UpdateAssessmentDto, Assessment>()  // Maps from canonical fields
```

**Verification:**
- ✅ No AutoMapper profile references alias properties
- ✅ All mappings target canonical entity fields (`ControlId`, `Type`, `Frequency`, `ResidualRisk`, `MitigationStrategy`)
- ✅ Single source of truth maintained: AutoMapper always uses canonical DTO properties

**Entity Field Alignment:**
- `Audit.Title` (entity) → `AuditDto.Name` (canonical) → alias `Title` (not mapped)
- `Control.ControlId` (entity) → `ControlDto.ControlId` (canonical) → alias `ControlNumber` (not mapped)
- `Risk.ResidualRisk` (entity) → `RiskDto.ResidualRisk` (canonical) → alias `RiskScore` (not mapped)

---

## 3. MVC Form Binding Behavior Review

### ✅ Model Binding Works for Both Property Names

**ASP.NET Core Model Binding:**
- MVC form binding works with **both** canonical and alias property names
- Examples:
  - `<input name="ControlNumber" />` → binds to `ControlDto.ControlNumber` (alias) → stores in `ControlId`
  - `<input name="ControlId" />` → binds to `ControlDto.ControlId` (canonical) → stores in `ControlId`
  - `<input name="RiskScore" />` → binds to `RiskDto.RiskScore` (alias) → stores in `ResidualRisk`
  - `<input name="ResidualRisk" />` → binds to `RiskDto.ResidualRisk` (canonical) → stores in `ResidualRisk`

**Razor Syntax (`asp-for`):**
- `asp-for="Model.ControlNumber"` → generates `name="ControlNumber"` → binds correctly
- `asp-for="Model.ControlId"` → generates `name="ControlId"` → binds correctly
- Both work identically due to property aliases

**Blazor Syntax (`@bind`):**
- `@bind="dto.ControlNumber"` → two-way binding works correctly
- `@bind="dto.ControlId"` → two-way binding works correctly

**Potential Edge Cases:**
⚠️ **Form with both names:** If a form submits both `ControlId` and `ControlNumber`, model binding will set both properties, but they map to the same backing field (last one wins, but this is benign duplication).

✅ **Mitigation:** Views should use **one naming convention consistently** (prefer canonical names in new views, but aliases work for backward compatibility).

---

## 4. Nullable Alias Setter Safety

### ✅ Safe Nullable Handling

**AuditDto.ScheduledDate (nullable) → PlannedStartDate (non-nullable):**

```csharp
public DateTime? ScheduledDate
{
    get => PlannedStartDate == default(DateTime) ? null : (DateTime?)PlannedStartDate;
    set
    {
        if (value.HasValue)
            PlannedStartDate = value.Value;
        // If null, preserve existing PlannedStartDate (domain requires non-null)
    }
}
```

**Safety:**
- ✅ Null assignment from model binding won't throw
- ✅ Preserves existing `PlannedStartDate` if null submitted (domain constraint)
- ✅ Alternative: Could set to `DateTime.MinValue` or throw domain exception if business rules require explicit scheduling

**Recommendation:** This is acceptable given domain constraint that audits must have a planned start date.

---

## 5. Consistency Checklist

### Property Alias Patterns

**Pattern Applied Consistently:**
```csharp
[System.Text.Json.Serialization.JsonIgnore]
[Newtonsoft.Json.JsonIgnore]
public string AliasName { get => CanonicalField; set => CanonicalField = value ?? string.Empty; }
```

**All Aliases Follow This Pattern:**
- ✅ `Title` → `Name`
- ✅ `AuditType` → `Type`
- ✅ `ControlNumber` → `ControlId`
- ✅ `ControlType` → `Type`
- ✅ `TestingFrequency` → `Frequency`
- ✅ `LastTestedDate` → `LastTestDate`
- ✅ `RiskScore` → `ResidualRisk`
- ✅ `TreatmentPlan` → `MitigationStrategy`
- ✅ `AssessmentType` → `Type`
- ✅ `CompletedDate` → `EndDate`
- ✅ `AssessorId` → `AssignedTo`
- ✅ `Notes` → `Description`

**String Aliases:** All use null-coalescing: `value ?? string.Empty`
**Nullable Aliases:** Handle null safely (see ScheduledDate above)

---

## 6. Files Modified Summary

1. **PolicyViolationAlert.razor**
   - Fixed type vs instance references
   - Removed unused IHttpContextAccessor
   - Used ToLowerInvariant() for culture safety

2. **CommonDtos.cs**
   - Added alias properties to `AuditDto` (4 aliases)
   - Added alias properties to `AssessmentDto` (4 aliases + 2 new fields)
   - Added missing properties to `PolicyDto` (3 properties)

3. **ControlDto.cs**
   - Added alias properties to `ControlDto` base class (4 aliases)
   - Added alias properties to `UpdateControlDto` (2 aliases)
   - Added missing `DataClassification` to base class

4. **RiskDto.cs**
   - Added alias properties to all 3 classes (`RiskDto`, `CreateRiskDto`, `UpdateRiskDto`) (2 aliases each)

5. **CCMController.cs**
   - Added logger to `AuditPackageController` (within CCMController.cs file)

6. **PolicyViolationParser.cs**
   - Fixed shadowed `prop` variable (renamed to `jsonProp`)
   - Removed duplicate `PolicyViolationInfo` class

---

## 7. Recommendations

### ✅ All Changes Verified Safe

**Serialization:** ✅ Dual-tagged, no duplicates in JSON
**AutoMapper:** ✅ Uses canonical fields only
**Model Binding:** ✅ Works with both property names
**Null Safety:** ✅ Handles nullable aliases correctly
**Type Safety:** ✅ Single PolicyViolationInfo definition

**No Breaking Changes:**
- ✅ Existing views using canonical names continue to work
- ✅ New views can use either naming convention
- ✅ API contracts remain stable (only canonical fields in JSON)

---

## Conclusion

All alias implementations are **consistent**, **safe**, and **backward-compatible**. The solution maintains API contract stability while preserving MVC form binding flexibility.

**Status:** ✅ Production Ready

---

## Maintenance and Hardening Measures

### 1. Duplicate Property Binding Detection

A `DuplicatePropertyBindingFilter` has been implemented to detect when both canonical and alias properties are bound from form submissions (e.g., both `ControlId` and `ControlNumber`). This filter logs warnings to help identify potential issues.

**Usage:** The filter is registered globally in `Program.cs` within the existing `AddControllersWithViews` call:
```csharp
builder.Services.AddControllersWithViews(options =>
{
    // Existing filters...
    options.Filters.Add<DuplicatePropertyBindingFilter>();
});
```

**Behavior:**
- Only runs on mutating HTTP methods (POST, PUT, PATCH)
- **API requests** (`/api/*` paths): Returns 400 BadRequest with ModelState errors if duplicates detected
- **MVC requests**: Logs warning but allows action to proceed (maintains backward compatibility)
- Filter is DI-constructible (no explicit `AddScoped` required)

### 2. ScheduledDate Semantics Documentation

The `ScheduledDate` alias property includes XML documentation explaining:
- `default(DateTime)` is treated as "null" (unset) for backward compatibility
- Domain layer should never legitimately use `DateTime.MinValue` or `default(DateTime)` as a real planned date
- Setter preserves existing value if null is submitted (respects domain constraint)

### 3. AutoMapper Stability Tests

Unit tests in `AutoMapperProfileTests.cs` ensure:
- AutoMapper profiles use canonical fields only
- Alias properties are not directly mapped by AutoMapper
- Prevents regressions where someone accidentally maps an alias instead of canonical field

**Run tests:**
```bash
dotnet test tests/GrcMvc.Tests/Unit/AutoMapperProfileTests.cs
```

### 4. Multi-Controller File Documentation

`CCMController.cs` includes a header comment explaining it intentionally contains multiple controller classes. This prevents accidental splitting during refactoring.

### 5. Future Recommendations

**Optional hardening (not currently implemented):**
- Add `[BindNever]` to alias properties if you want to prevent alias-post compatibility (only use aliases for display/read, not form submission)
- Consider standardizing new views to use canonical property names only
- Keep aliases only for legacy view compatibility

**Best Practices:**
- When creating new DTOs, prefer canonical field names from the start
- If adding aliases, always dual-tag with `[JsonIgnore]` attributes
- Update this document when adding new alias patterns
