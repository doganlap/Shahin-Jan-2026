# 📋 Complete Missing Components List

**Generated:** 2026-01-01  
**Comprehensive Audit:** All missing functions, registrations, endpoints, and configurations

---

## 🔴 CRITICAL MISSING: API ENDPOINTS (GetListAsync)

### 13 AppServices Missing GetListAsync:

1. **EvidenceAppService** ❌
   - Missing: `Task<PagedResultDto<EvidenceDto>> GetListAsync(EvidenceListInputDto input)`
   - Missing DTO: `EvidenceListInputDto.cs`

2. **AssessmentAppService** ❌
   - Missing: `Task<PagedResultDto<AssessmentDto>> GetListAsync(AssessmentListInputDto input)`
   - Missing DTO: `AssessmentListInputDto.cs`

3. **AuditAppService** ❌
   - Missing: `Task<PagedResultDto<AuditDto>> GetListAsync(AuditListInputDto input)`
   - Missing DTO: `AuditListInputDto.cs`

4. **RiskAppService** ❌
   - Missing: `Task<PagedResultDto<RiskDto>> GetListAsync(RiskListInputDto input)`
   - Missing DTO: `RiskListInputDto.cs`

5. **ActionPlanAppService** ❌
   - Missing: `Task<PagedResultDto<ActionPlanDto>> GetListAsync(ActionPlanListInputDto input)`
   - Missing DTO: `ActionPlanListInputDto.cs`
   - **ALSO MISSING:** `UpdateAsync` method (only has AssignAsync, CloseAsync)

6. **PolicyDocumentAppService** ❌
   - Missing: `Task<PagedResultDto<PolicyDocumentDto>> GetListAsync(PolicyDocumentListInputDto input)`
   - Missing DTO: `PolicyDocumentListInputDto.cs`

7. **ControlAssessmentAppService** ❌
   - Missing: `Task<PagedResultDto<ControlAssessmentDto>> GetListAsync(ControlAssessmentListInputDto input)`
   - Missing DTO: `ControlAssessmentListInputDto.cs`

8. **RegulatoryFrameworkAppService** ❌
   - Missing: `Task<PagedResultDto<RegulatoryFrameworkDto>> GetListAsync(RegulatoryFrameworkListInputDto input)`
   - Missing DTO: `RegulatoryFrameworkListInputDto.cs`

9. **RegulatorAppService** ❌
   - Missing: `Task<PagedResultDto<RegulatorDto>> GetListAsync(RegulatorListInputDto input)`
   - Missing DTO: `RegulatorListInputDto.cs`

10. **VendorAppService** ❌
    - Missing: `Task<PagedResultDto<VendorDto>> GetListAsync(VendorListInputDto input)`
    - Missing DTO: `VendorListInputDto.cs`
    - **ALSO MISSING:** `AssessAsync` method (permission exists: Vendors.Assess)

11. **ComplianceCalendarAppService** ❌
    - Missing: `Task<PagedResultDto<ComplianceEventDto>> GetListAsync(ComplianceEventListInputDto input)`
    - Missing DTO: `ComplianceEventListInputDto.cs`

12. **WorkflowAppService** ❌
    - Missing: `Task<PagedResultDto<WorkflowDto>> GetListAsync(WorkflowListInputDto input)`
    - Missing DTO: `WorkflowListInputDto.cs`

13. **NotificationAppService** ❌
    - Missing: `Task<PagedResultDto<NotificationDto>> GetListAsync(NotificationListInputDto input)`
    - Missing DTO: `NotificationListInputDto.cs`
    - **ALSO MISSING:** `GetUnreadCountAsync`, `MarkAllAsReadAsync`

---

## 🔴 CRITICAL MISSING: SERVICE REGISTRATIONS

### Missing in GrcApplicationModule.cs:

```csharp
// ❌ MISSING:
services.AddScoped<IEnvironmentProvider, EnvironmentProvider>();
services.AddScoped<IRoleResolver, RoleResolver>();
```

**Current Status:** These services exist but are NOT registered. They're accessed via LazyServiceProvider which is inefficient.

### Missing in GrcBlazorModule.cs:

```csharp
// ❌ MISSING:
// HttpClient configuration (basic exists, but may need more)
// Authorization services
// Localization services
```

### Missing in GrcEntityFrameworkCoreModule.cs:

```csharp
// ❌ MISSING:
Configure<AbpDbContextOptions>(options =>
{
    options.Configure(ctx =>
    {
        ctx.DbContextOptions.UseSqlServer();
    });
});
```

---

## 🔴 CRITICAL MISSING: CONFIGURATIONS

### Missing in appsettings.json:

```json
{
  "ConnectionStrings": {
    "Default": "...", // ✅ EXISTS
    "Redis": "...", // ❌ MISSING
    "BlobStorage": "..." // ❌ MISSING
  },
  "Authentication": { // ❌ MISSING ENTIRE SECTION
    "Jwt": {
      "SecretKey": "...",
      "Issuer": "...",
      "Audience": "...",
      "Expiration": 3600
    }
  },
  "Cors": { // ❌ MISSING ENTIRE SECTION
    "AllowedOrigins": ["http://localhost:5000", "https://localhost:5001"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["*"]
  },
  "MultiTenancy": { // ❌ MISSING ENTIRE SECTION
    "IsEnabled": true,
    "DatabaseStyle": "Hybrid"
  },
  "Email": { // ❌ MISSING ENTIRE SECTION
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "UserName": "...",
      "Password": "...",
      "EnableSsl": true
    }
  },
  "FileStorage": { // ❌ MISSING ENTIRE SECTION
    "Provider": "Local",
    "Path": "wwwroot/uploads"
  },
  "Cache": { // ❌ MISSING ENTIRE SECTION
    "Provider": "Memory",
    "ConnectionString": ""
  },
  "Swagger": { // ❌ MISSING ENTIRE SECTION
    "Title": "GRC API",
    "Version": "v1",
    "Description": "GRC System API"
  }
}
```

### Missing Configuration Files:

1. ❌ **appsettings.Development.json**
2. ❌ **appsettings.Production.json**
3. ❌ **appsettings.Staging.json**

### Missing in Program.cs:

```csharp
// ❌ MISSING:
builder.Services.AddCors(options => { ... });
builder.Services.AddAuthentication(...);
builder.Services.AddAuthorization(...);
builder.Services.AddSwaggerGen(...);
builder.Services.AddHealthChecks(...);
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
```

---

## 🟡 HIGH PRIORITY MISSING: FUNCTIONS

### Missing Update Methods:

1. **ActionPlanAppService** ❌
   - Missing: `UpdateAsync(Guid id, UpdateActionPlanDto input)`
   - Currently only has: CreateAsync, AssignAsync, CloseAsync, GetAsync, DeleteAsync

### Missing Business Logic Methods:

2. **VendorAppService** ❌
   - Missing: `AssessAsync(Guid id, VendorAssessmentDto input)`
   - Permission exists: `GrcPermissions.Vendors.Assess`

3. **NotificationAppService** ❌
   - Missing: `GetUnreadCountAsync()`
   - Missing: `MarkAllAsReadAsync()`

4. **ComplianceCalendarAppService** ❌
   - Missing: `GetByDateRangeAsync(DateTime start, DateTime end)`
   - Missing: `GetUpcomingAsync(int days)`
   - Missing: `GetOverdueAsync()`

5. **WorkflowAppService** ❌
   - Missing: `ExecuteAsync(Guid id, WorkflowExecutionDto input)`
   - Missing: `GetStatusAsync(Guid id)`

### Missing Labels Update in UpdateAsync:

**Problem:** Most UpdateAsync methods don't update Labels dictionary.

**Affected Services:**
- AssessmentAppService.UpdateAsync ❌
- AuditAppService.UpdateAsync ❌
- RiskAppService.UpdateAsync ❌
- ActionPlanAppService.UpdateAsync ❌ (method doesn't exist)
- ControlAssessmentAppService.UpdateAsync ❌
- RegulatoryFrameworkAppService.UpdateAsync ❌
- RegulatorAppService.UpdateAsync ❌
- VendorAppService.UpdateAsync ❌
- ComplianceCalendarAppService.UpdateAsync ❌
- WorkflowAppService.UpdateAsync ❌
- NotificationAppService.UpdateAsync ❌

---

## 🟡 HIGH PRIORITY MISSING: INTERFACE UPDATES

### Missing Methods in Interfaces:

1. **IEvidenceAppService** ❌
   - Missing: `Task<PagedResultDto<EvidenceDto>> GetListAsync(EvidenceListInputDto input)`

2. **IAssessmentAppService** ❌
   - Missing: `Task<PagedResultDto<AssessmentDto>> GetListAsync(AssessmentListInputDto input)`

3. **IAuditAppService** ❌
   - Missing: `Task<PagedResultDto<AuditDto>> GetListAsync(AuditListInputDto input)`

4. **IRiskAppService** ❌
   - Missing: `Task<PagedResultDto<RiskDto>> GetListAsync(RiskListInputDto input)`

5. **IActionPlanAppService** ❌
   - Missing: `Task<PagedResultDto<ActionPlanDto>> GetListAsync(ActionPlanListInputDto input)`
   - Missing: `Task<ActionPlanDto> UpdateAsync(Guid id, UpdateActionPlanDto input)`

6. **IPolicyDocumentAppService** ❌
   - Missing: `Task<PagedResultDto<PolicyDocumentDto>> GetListAsync(PolicyDocumentListInputDto input)`

7. **IControlAssessmentAppService** ❌
   - Missing: `Task<PagedResultDto<ControlAssessmentDto>> GetListAsync(ControlAssessmentListInputDto input)`

8. **IRegulatoryFrameworkAppService** ❌
   - Missing: `Task<PagedResultDto<RegulatoryFrameworkDto>> GetListAsync(RegulatoryFrameworkListInputDto input)`

9. **IRegulatorAppService** ❌
   - Missing: `Task<PagedResultDto<RegulatorDto>> GetListAsync(RegulatorListInputDto input)`

10. **IVendorAppService** ❌
    - Missing: `Task<PagedResultDto<VendorDto>> GetListAsync(VendorListInputDto input)`
    - Missing: `Task<VendorDto> AssessAsync(Guid id, VendorAssessmentDto input)`

11. **IComplianceCalendarAppService** ❌
    - Missing: `Task<PagedResultDto<ComplianceEventDto>> GetListAsync(ComplianceEventListInputDto input)`
    - Missing: `Task<List<ComplianceEventDto>> GetByDateRangeAsync(DateTime start, DateTime end)`
    - Missing: `Task<List<ComplianceEventDto>> GetUpcomingAsync(int days)`
    - Missing: `Task<List<ComplianceEventDto>> GetOverdueAsync()`

12. **IWorkflowAppService** ❌
    - Missing: `Task<PagedResultDto<WorkflowDto>> GetListAsync(WorkflowListInputDto input)`
    - Missing: `Task<WorkflowExecutionResultDto> ExecuteAsync(Guid id, WorkflowExecutionDto input)`
    - Missing: `Task<WorkflowStatusDto> GetStatusAsync(Guid id)`

13. **INotificationAppService** ❌
    - Missing: `Task<PagedResultDto<NotificationDto>> GetListAsync(NotificationListInputDto input)`
    - Missing: `Task<int> GetUnreadCountAsync()`
    - Missing: `Task MarkAllAsReadAsync()`

---

## 🟠 MEDIUM PRIORITY MISSING: BLazor PAGES

### Missing Pages (35+):

**Frameworks:**
- ❌ Pages/Frameworks/Index.razor
- ❌ Pages/Frameworks/Create.razor
- ❌ Pages/Frameworks/Edit.razor

**Regulators:**
- ❌ Pages/Regulators/Index.razor
- ❌ Pages/Regulators/Create.razor
- ❌ Pages/Regulators/Edit.razor

**Assessments:**
- ❌ Pages/Assessments/Index.razor
- ❌ Pages/Assessments/Create.razor
- ❌ Pages/Assessments/Edit.razor

**Control Assessments:**
- ❌ Pages/ControlAssessments/Index.razor
- ❌ Pages/ControlAssessments/Create.razor
- ❌ Pages/ControlAssessments/Edit.razor

**Risks:**
- ❌ Pages/Risks/Index.razor
- ❌ Pages/Risks/Create.razor
- ❌ Pages/Risks/Edit.razor

**Audits:**
- ❌ Pages/Audits/Index.razor
- ❌ Pages/Audits/Create.razor
- ❌ Pages/Audits/Edit.razor

**Action Plans:**
- ❌ Pages/ActionPlans/Index.razor
- ❌ Pages/ActionPlans/Create.razor
- ❌ Pages/ActionPlans/Edit.razor

**Policies:**
- ❌ Pages/Policies/Index.razor
- ❌ Pages/Policies/Create.razor
- ❌ Pages/Policies/Edit.razor

**Compliance Calendar:**
- ❌ Pages/ComplianceCalendar/Index.razor

**Workflow:**
- ❌ Pages/Workflow/Index.razor
- ❌ Pages/Workflow/Create.razor
- ❌ Pages/Workflow/Edit.razor

**Notifications:**
- ❌ Pages/Notifications/Index.razor

**Vendors:**
- ❌ Pages/Vendors/Index.razor
- ❌ Pages/Vendors/Create.razor
- ❌ Pages/Vendors/Edit.razor

**Reports:**
- ❌ Pages/Reports/Index.razor

**Integrations:**
- ❌ Pages/Integrations/Index.razor

**Subscriptions:**
- ❌ Pages/Subscriptions/Index.razor

---

## 🟠 MEDIUM PRIORITY MISSING: ERROR HANDLING

### Missing Components:

1. ❌ **GlobalExceptionHandlerMiddleware.cs**
2. ❌ **ErrorResponseDto.cs** (standardized error format)
3. ❌ **ErrorDialog.razor** (reusable error dialog component)
4. ❌ **ValidationErrorHandler.cs** (FluentValidation integration)
5. ❌ **ErrorToastService.cs** (toast notification service)

### Missing in Blazor Pages:

- ❌ Replace `Console.WriteLine` with proper error handling
- ❌ Add loading states
- ❌ Add empty state components
- ❌ Add confirmation dialogs for delete operations

---

## 🟢 LOW PRIORITY MISSING: ADDITIONAL FEATURES

### Missing DTOs:

1. ❌ **UpdateActionPlanDto.cs** (for ActionPlanAppService.UpdateAsync)
2. ❌ **VendorAssessmentDto.cs** (for VendorAppService.AssessAsync)
3. ❌ **WorkflowExecutionDto.cs** (for WorkflowAppService.ExecuteAsync)
4. ❌ **WorkflowExecutionResultDto.cs**
5. ❌ **WorkflowStatusDto.cs**

### Missing Helper Methods:

1. ❌ **GetCountAsync** methods (optional, for statistics)
2. ❌ **Bulk operations** (BulkDeleteAsync, BulkUpdateAsync)
3. ❌ **Export methods** (ExportToExcelAsync, ExportToPdfAsync)
4. ❌ **Search methods** (SearchAsync with advanced filters)

---

## 📊 SUMMARY BY PRIORITY

### 🔴 CRITICAL (Must Fix - Blocks API Functionality):
- **13 GetListAsync methods** - Cannot list entities
- **13 ListInputDto classes** - Missing DTOs
- **13 Interface updates** - Interfaces don't match implementations
- **2 Service registrations** - IEnvironmentProvider, IRoleResolver
- **1 UpdateAsync method** - ActionPlanAppService missing UpdateAsync

**Total Critical:** 42 items

### 🟡 HIGH (Should Fix - Affects Functionality):
- **11 Labels updates** - UpdateAsync methods don't update Labels
- **5 Business logic methods** - Vendor.AssessAsync, Notification methods, etc.
- **8 Configuration sections** - CORS, Auth, MultiTenancy, etc.
- **3 Configuration files** - Environment-specific appsettings

**Total High:** 27 items

### 🟠 MEDIUM (Nice to Have):
- **35+ Blazor pages** - UI components
- **5 Error handling components** - Better UX

**Total Medium:** 40+ items

### 🟢 LOW (Future Enhancements):
- **5 Additional DTOs** - For advanced features
- **4 Helper methods** - Bulk operations, exports

**Total Low:** 9 items

---

## 🎯 TOTAL MISSING: 118+ Components

**Breakdown:**
- Critical: 42 items
- High: 27 items
- Medium: 40+ items
- Low: 9 items

---

## ✅ FIX PRIORITY ORDER

1. **Fix Service Registrations** (5 min)
2. **Add GetListAsync to all AppServices** (2-3 hours)
3. **Create ListInputDto classes** (30 min)
4. **Update Interfaces** (15 min)
5. **Add UpdateAsync to ActionPlanAppService** (10 min)
6. **Fix Labels updates in UpdateAsync methods** (30 min)
7. **Add missing business logic methods** (1 hour)
8. **Add configurations** (1 hour)
9. **Create Blazor pages** (4-6 hours)
10. **Add error handling** (2 hours)

**Estimated Total Time:** 12-15 hours
