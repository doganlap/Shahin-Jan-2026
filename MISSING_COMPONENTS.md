# 🔍 Missing Components Report

**Generated:** 2026-01-01  
**Status:** Comprehensive audit of missing functionality

---

## 1. ❌ MISSING API ENDPOINTS (GetListAsync Methods)

### Problem
Most AppServices are missing `GetListAsync` methods for pagination and listing. Only SubscriptionAppService (inherits CrudAppService) and Admin services have this.

### Missing List Endpoints (10 services):

#### 1. EvidenceAppService ❌
**Missing:**
- `Task<PagedResultDto<EvidenceDto>> GetListAsync(EvidenceListInputDto input)`

**Impact:** Cannot list/paginate evidence items

#### 2. AssessmentAppService ❌
**Missing:**
- `Task<PagedResultDto<AssessmentDto>> GetListAsync(AssessmentListInputDto input)`

**Impact:** Cannot list/paginate assessments

#### 3. AuditAppService ❌
**Missing:**
- `Task<PagedResultDto<AuditDto>> GetListAsync(AuditListInputDto input)`

**Impact:** Cannot list/paginate audits

#### 4. RiskAppService ❌
**Missing:**
- `Task<PagedResultDto<RiskDto>> GetListAsync(RiskListInputDto input)`

**Impact:** Cannot list/paginate risks

#### 5. ActionPlanAppService ❌
**Missing:**
- `Task<PagedResultDto<ActionPlanDto>> GetListAsync(ActionPlanListInputDto input)`

**Impact:** Cannot list/paginate action plans

#### 6. PolicyDocumentAppService ❌
**Missing:**
- `Task<PagedResultDto<PolicyDocumentDto>> GetListAsync(PolicyDocumentListInputDto input)`

**Impact:** Cannot list/paginate policy documents

#### 7. ControlAssessmentAppService ❌
**Missing:**
- `Task<PagedResultDto<ControlAssessmentDto>> GetListAsync(ControlAssessmentListInputDto input)`

**Impact:** Cannot list/paginate control assessments

#### 8. RegulatoryFrameworkAppService ❌
**Missing:**
- `Task<PagedResultDto<RegulatoryFrameworkDto>> GetListAsync(RegulatoryFrameworkListInputDto input)`

**Impact:** Cannot list/paginate regulatory frameworks

#### 9. RegulatorAppService ❌
**Missing:**
- `Task<PagedResultDto<RegulatorDto>> GetListAsync(RegulatorListInputDto input)`

**Impact:** Cannot list/paginate regulators

#### 10. VendorAppService ❌
**Missing:**
- `Task<PagedResultDto<VendorDto>> GetListAsync(VendorListInputDto input)`

**Impact:** Cannot list/paginate vendors

#### 11. ComplianceCalendarAppService ❌
**Missing:**
- `Task<PagedResultDto<ComplianceEventDto>> GetListAsync(ComplianceEventListInputDto input)`

**Impact:** Cannot list/paginate compliance events

#### 12. WorkflowAppService ❌
**Missing:**
- `Task<PagedResultDto<WorkflowDto>> GetListAsync(WorkflowListInputDto input)`

**Impact:** Cannot list/paginate workflows

#### 13. NotificationAppService ❌
**Missing:**
- `Task<PagedResultDto<NotificationDto>> GetListAsync(NotificationListInputDto input)`

**Impact:** Cannot list/paginate notifications

---

## 2. ❌ MISSING DTOs (ListInputDto Classes)

### Missing ListInputDto Classes (13 DTOs):

1. ❌ `EvidenceListInputDto.cs`
2. ❌ `AssessmentListInputDto.cs`
3. ❌ `AuditListInputDto.cs`
4. ❌ `RiskListInputDto.cs`
5. ❌ `ActionPlanListInputDto.cs`
6. ❌ `PolicyDocumentListInputDto.cs`
7. ❌ `ControlAssessmentListInputDto.cs`
8. ❌ `RegulatoryFrameworkListInputDto.cs`
9. ❌ `RegulatorListInputDto.cs`
10. ❌ `VendorListInputDto.cs`
11. ❌ `ComplianceEventListInputDto.cs`
12. ❌ `WorkflowListInputDto.cs`
13. ❌ `NotificationListInputDto.cs`

**Location:** Should be in `Grc.Application.Contracts/{Entity}/` folders

---

## 3. ❌ MISSING INTERFACE METHODS

### Missing in IEvidenceAppService:
- `Task<PagedResultDto<EvidenceDto>> GetListAsync(EvidenceListInputDto input)`

### Missing in IAssessmentAppService:
- `Task<PagedResultDto<AssessmentDto>> GetListAsync(AssessmentListInputDto input)`

### Missing in IAuditAppService:
- `Task<PagedResultDto<AuditDto>> GetListAsync(AuditListInputDto input)`

### Missing in IRiskAppService:
- `Task<PagedResultDto<RiskDto>> GetListAsync(RiskListInputDto input)`

### Missing in IActionPlanAppService:
- `Task<PagedResultDto<ActionPlanDto>> GetListAsync(ActionPlanListInputDto input)`

### Missing in IPolicyDocumentAppService:
- `Task<PagedResultDto<PolicyDocumentDto>> GetListAsync(PolicyDocumentListInputDto input)`

### Missing in IControlAssessmentAppService:
- `Task<PagedResultDto<ControlAssessmentDto>> GetListAsync(ControlAssessmentListInputDto input)`

### Missing in IRegulatoryFrameworkAppService:
- `Task<PagedResultDto<RegulatoryFrameworkDto>> GetListAsync(RegulatoryFrameworkListInputDto input)`

### Missing in IRegulatorAppService:
- `Task<PagedResultDto<RegulatorDto>> GetListAsync(RegulatorListInputDto input)`

### Missing in IVendorAppService:
- `Task<PagedResultDto<VendorDto>> GetListAsync(VendorListInputDto input)`

### Missing in IComplianceCalendarAppService:
- `Task<PagedResultDto<ComplianceEventDto>> GetListAsync(ComplianceEventListInputDto input)`

### Missing in IWorkflowAppService:
- `Task<PagedResultDto<WorkflowDto>> GetListAsync(WorkflowListInputDto input)`

### Missing in INotificationAppService:
- `Task<PagedResultDto<NotificationDto>> GetListAsync(NotificationListInputDto input)`

---

## 4. ❌ MISSING SERVICE REGISTRATIONS

### Missing in GrcApplicationModule.cs:

#### Policy Services ✅ (Already registered)
- ✅ IPolicyStore
- ✅ IPolicyEnforcer
- ✅ IPolicyAuditLogger

#### Missing Supporting Services:
- ❌ **IEnvironmentProvider** - Not registered (used in BasePolicyAppService)
- ❌ **EnvironmentProvider** - Implementation not registered
- ❌ **IRoleResolver** - Not registered (used in BasePolicyAppService)
- ❌ **RoleResolver** - Implementation not registered

**Current Status:** These are accessed via `LazyServiceProvider` but should be properly registered.

### Missing in GrcBlazorModule.cs:

#### Already Registered ✅:
- ✅ IMenuContributor

#### Missing:
- ❌ **HttpClient** configuration for API calls
- ❌ **Authorization** services configuration
- ❌ **Localization** configuration
- ❌ **Theme** configuration (if using ABP themes)

### Missing in GrcDomainModule.cs:

#### Missing:
- ❌ **Seed Contributors** registration (GrcRoleDataSeedContributor, GrcAdminUserDataSeedContributor)
- ❌ **Permission Definition Provider** registration (GrcPermissionDefinitionProvider)

**Note:** Permission providers are usually auto-discovered, but should be verified.

### Missing in GrcEntityFrameworkCoreModule.cs:

#### Already Registered ✅:
- ✅ DbContext with default repositories

#### Missing:
- ❌ **Connection String** configuration validation
- ❌ **Database Provider** selection logic
- ❌ **Migration** configuration

---

## 5. ❌ MISSING CONFIGURATIONS

### Missing in appsettings.json:

#### Already Configured ✅:
- ✅ ConnectionStrings.Default
- ✅ Logging
- ✅ Policy configuration
- ✅ AdminUser

#### Missing:
- ❌ **CORS** configuration
- ❌ **Authentication** configuration (JWT/OIDC)
- ❌ **Authorization** policies
- ❌ **MultiTenancy** configuration
- ❌ **Identity** configuration
- ❌ **Email** settings (SMTP)
- ❌ **File Storage** configuration
- ❌ **Cache** configuration (Redis/Memory)
- ❌ **Rate Limiting** configuration
- ❌ **Swagger/OpenAPI** configuration

### Missing Configuration Files:

1. ❌ **appsettings.Development.json**
2. ❌ **appsettings.Production.json**
3. ❌ **appsettings.Staging.json**

### Missing in Program.cs:

#### Already Created ✅:
- ✅ Basic ABP module registration
- ✅ HttpClient configuration

#### Missing:
- ❌ **CORS** middleware
- ❌ **Authentication** middleware
- ❌ **Authorization** middleware
- ❌ **Exception Handling** middleware
- ❌ **Swagger** configuration
- ❌ **Health Checks** configuration
- ❌ **Logging** configuration
- ❌ **Database** migration on startup (optional)

---

## 6. ❌ MISSING BLazor PAGES

### Missing Pages (13+):

1. ❌ **Frameworks/Index.razor** - List regulatory frameworks
2. ❌ **Frameworks/Create.razor** - Create framework
3. ❌ **Frameworks/Edit.razor** - Edit framework
4. ❌ **Regulators/Index.razor** - List regulators
5. ❌ **Regulators/Create.razor** - Create regulator
6. ❌ **Regulators/Edit.razor** - Edit regulator
7. ❌ **Assessments/Index.razor** - List assessments
8. ❌ **Assessments/Create.razor** - Create assessment
9. ❌ **Assessments/Edit.razor** - Edit assessment
10. ❌ **ControlAssessments/Index.razor** - List control assessments
11. ❌ **ControlAssessments/Create.razor** - Create control assessment
12. ❌ **ControlAssessments/Edit.razor** - Edit control assessment
13. ❌ **Risks/Index.razor** - List risks
14. ❌ **Risks/Create.razor** - Create risk
15. ❌ **Risks/Edit.razor** - Edit risk
16. ❌ **Audits/Index.razor** - List audits
17. ❌ **Audits/Create.razor** - Create audit
18. ❌ **Audits/Edit.razor** - Edit audit
19. ❌ **ActionPlans/Index.razor** - List action plans
20. ❌ **ActionPlans/Create.razor** - Create action plan
21. ❌ **ActionPlans/Edit.razor** - Edit action plan
22. ❌ **Policies/Index.razor** - List policy documents
23. ❌ **Policies/Create.razor** - Create policy document
24. ❌ **Policies/Edit.razor** - Edit policy document
25. ❌ **ComplianceCalendar/Index.razor** - Compliance calendar view
26. ❌ **Workflow/Index.razor** - List workflows
27. ❌ **Workflow/Create.razor** - Create workflow
28. ❌ **Workflow/Edit.razor** - Edit workflow
29. ❌ **Notifications/Index.razor** - List notifications
30. ❌ **Vendors/Index.razor** - List vendors
31. ❌ **Vendors/Create.razor** - Create vendor
32. ❌ **Vendors/Edit.razor** - Edit vendor
33. ❌ **Reports/Index.razor** - Reports dashboard
34. ❌ **Integrations/Index.razor** - Integrations management
35. ❌ **Subscriptions/Index.razor** - List subscriptions

**Total Missing:** 35+ pages

---

## 7. ❌ MISSING FUNCTIONS IN EXISTING SERVICES

### EvidenceAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ `GetCountAsync` - Get total count (optional)

### AssessmentAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync

### AuditAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync

### RiskAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync

### ActionPlanAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync
- ❌ Missing `UpdateAsync` method (only has AssignAsync and CloseAsync)

### PolicyDocumentAppService:
- ❌ `GetListAsync` - List with pagination

### ControlAssessmentAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync

### RegulatoryFrameworkAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync

### RegulatorAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync

### VendorAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync
- ❌ Missing `AssessAsync` method (permission exists: Vendors.Assess)

### ComplianceCalendarAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync
- ❌ Missing calendar-specific methods (GetByDateRange, GetUpcoming, etc.)

### WorkflowAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync
- ❌ Missing workflow execution methods (ExecuteAsync, GetStatusAsync)

### NotificationAppService:
- ❌ `GetListAsync` - List with pagination
- ❌ Missing Labels update in UpdateAsync
- ❌ Missing `GetUnreadCountAsync` method
- ❌ Missing `MarkAllAsReadAsync` method

---

## 8. ❌ MISSING REGISTRATIONS IN MODULES

### GrcApplicationModule.cs Missing:
```csharp
// Missing registrations:
services.AddScoped<IEnvironmentProvider, EnvironmentProvider>();
services.AddScoped<IRoleResolver, RoleResolver>();
```

### GrcBlazorModule.cs Missing:
```csharp
// Missing registrations:
services.AddHttpClient();
services.AddAuthorizationCore();
// ABP theme services (if using)
```

### GrcDomainModule.cs Missing:
```csharp
// Missing registrations:
context.Services.Configure<GrcOptions>(options => { });
// Seed contributors are usually auto-discovered, but verify
```

### GrcEntityFrameworkCoreModule.cs Missing:
```csharp
// Missing configurations:
Configure<AbpDbContextOptions>(options =>
{
    options.UseSqlServer();
});
```

---

## 9. ❌ MISSING CONFIGURATION VALUES

### appsettings.json Missing Sections:

```json
{
  "ConnectionStrings": {
    "Default": "...", // ✅ Exists
    "Redis": "...", // ❌ Missing
    "BlobStorage": "..." // ❌ Missing
  },
  "Authentication": {
    "Jwt": {
      "SecretKey": "...", // ❌ Missing
      "Issuer": "...", // ❌ Missing
      "Audience": "..." // ❌ Missing
    }
  },
  "Cors": {
    "AllowedOrigins": ["..."], // ❌ Missing
    "AllowedMethods": ["..."], // ❌ Missing
    "AllowedHeaders": ["..."] // ❌ Missing
  },
  "MultiTenancy": {
    "IsEnabled": true, // ❌ Missing
    "DatabaseStyle": "..." // ❌ Missing
  },
  "Email": {
    "Smtp": {
      "Host": "...", // ❌ Missing
      "Port": 587, // ❌ Missing
      "UserName": "...", // ❌ Missing
      "Password": "..." // ❌ Missing
    }
  },
  "FileStorage": {
    "Provider": "...", // ❌ Missing
    "Path": "..." // ❌ Missing
  },
  "Cache": {
    "Provider": "...", // ❌ Missing
    "ConnectionString": "..." // ❌ Missing
  }
}
```

---

## 10. ❌ MISSING ERROR HANDLING

### Missing Error Handling Components:

1. ❌ **Global Exception Handler** middleware
2. ❌ **Error Response DTO** (standardized error format)
3. ❌ **Error Dialog Component** in Blazor (reusable)
4. ❌ **Validation Error Handler** (FluentValidation integration)
5. ❌ **Policy Violation Handler** (already have PolicyViolationDialog, but need integration)

### Missing in Blazor Pages:
- ❌ Proper error handling (currently using Console.WriteLine)
- ❌ Error toast notifications
- ❌ Loading states
- ❌ Empty state components

---

## 📊 SUMMARY

| Category | Missing Count | Critical |
|----------|---------------|----------|
| **GetListAsync Methods** | 13 | 🔴 Yes |
| **ListInputDto Classes** | 13 | 🔴 Yes |
| **Interface Methods** | 13 | 🔴 Yes |
| **Service Registrations** | 4 | 🟡 Medium |
| **Configuration Sections** | 8+ | 🟡 Medium |
| **Blazor Pages** | 35+ | 🟠 Low |
| **Error Handling** | 5 | 🟡 Medium |
| **Missing Functions** | 20+ | 🟡 Medium |

**Total Missing Items:** 110+ components

---

## 🎯 PRIORITY FIXES

### 🔴 CRITICAL (Must Fix):
1. Add GetListAsync to all 13 AppServices
2. Create 13 ListInputDto classes
3. Update 13 interfaces with GetListAsync
4. Register IEnvironmentProvider and IRoleResolver

### 🟡 HIGH (Should Fix):
5. Add UpdateAsync to ActionPlanAppService
6. Add AssessAsync to VendorAppService
7. Add GetUnreadCountAsync to NotificationAppService
8. Fix Labels update in all UpdateAsync methods
9. Add CORS configuration
10. Add Authentication configuration

### 🟠 MEDIUM (Nice to Have):
11. Create missing Blazor pages
12. Add error handling components
13. Add environment-specific appsettings files
14. Add Swagger configuration

---

## ✅ VERIFICATION CHECKLIST

- [ ] All GetListAsync methods implemented
- [ ] All ListInputDto classes created
- [ ] All interfaces updated
- [ ] All service registrations complete
- [ ] All configurations added
- [ ] All Blazor pages created
- [ ] Error handling implemented
- [ ] Missing functions added
