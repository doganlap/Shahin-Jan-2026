# 📋 Remaining Actions - GRC System

## 🔴 CRITICAL (Must Fix Before Production)

### 1. Build System & Infrastructure
- [ ] **Fix ABP NuGet Package References** - Verify package names and versions
- [ ] **Create DbContext** (`GrcDbContext.cs`) - Entity Framework Core context
- [ ] **Create EntityFrameworkCore Project** - New project for EF Core
- [ ] **Implement Repository Classes** (10+ repositories):
  - [ ] `EfCoreEvidenceRepository.cs`
  - [ ] `EfCoreAssessmentRepository.cs`
  - [ ] `EfCoreRiskRepository.cs`
  - [ ] `EfCoreAuditRepository.cs`
  - [ ] `EfCoreActionPlanRepository.cs`
  - [ ] `EfCorePolicyDocumentRepository.cs`
  - [ ] `EfCoreControlAssessmentRepository.cs` (when entity exists)
  - [ ] `EfCoreRegulatoryFrameworkRepository.cs` (when entity exists)
  - [ ] `EfCoreRegulatorRepository.cs` (when entity exists)
  - [ ] `EfCoreVendorRepository.cs` (when entity exists)
  - [ ] `EfCoreComplianceEventRepository.cs` (when entity exists)
  - [ ] `EfCoreWorkflowRepository.cs` (when entity exists)
  - [ ] `EfCoreNotificationRepository.cs` (when entity exists)
- [ ] **Create Program.cs** - Application entry point with ABP host configuration
- [ ] **Configure Database Connection Strings** - In appsettings.json
- [ ] **Create Initial Migration** - EF Core migration scripts
- [ ] **Register DbContext in DI** - In module configuration

### 2. Missing Domain Entities (7 entities)
- [ ] **ControlAssessment.cs** - Entity + Repository interface
- [ ] **RegulatoryFramework.cs** - Entity + Repository interface
- [ ] **Regulator.cs** - Entity + Repository interface
- [ ] **Vendor.cs** - Entity + Repository interface
- [ ] **ComplianceEvent.cs** - Entity + Repository interface
- [ ] **Workflow.cs** - Entity + Repository interface
- [ ] **Notification.cs** - Entity + Repository interface

### 3. Missing Application Services (7 services)
- [ ] **RegulatoryFrameworkAppService.cs** - Full CRUD + DTOs
- [ ] **RegulatorAppService.cs** - Full CRUD + DTOs
- [ ] **VendorAppService.cs** - Full CRUD + DTOs
- [ ] **ComplianceCalendarAppService.cs** - Full CRUD + DTOs
- [ ] **WorkflowAppService.cs** - Full CRUD + DTOs
- [ ] **NotificationAppService.cs** - Full CRUD + DTOs
- [ ] **ControlAssessmentAppService.cs** - Full CRUD + DTOs
- [ ] **Fix SubscriptionAppService.cs** - Currently throws NotImplementedException

### 4. Missing DTOs & Contracts
- [ ] **Create DTOs for missing entities** (in Application.Contracts):
  - [ ] `ControlAssessmentDto.cs` + `CreateControlAssessmentDto.cs` + `UpdateControlAssessmentDto.cs`
  - [ ] `RegulatoryFrameworkDto.cs` + `CreateRegulatoryFrameworkDto.cs` + `UpdateRegulatoryFrameworkDto.cs`
  - [ ] `RegulatorDto.cs` + `CreateRegulatorDto.cs` + `UpdateRegulatorDto.cs`
  - [ ] `VendorDto.cs` + `CreateVendorDto.cs` + `UpdateVendorDto.cs`
  - [ ] `ComplianceEventDto.cs` + `CreateComplianceEventDto.cs` + `UpdateComplianceEventDto.cs`
  - [ ] `WorkflowDto.cs` + `CreateWorkflowDto.cs` + `UpdateWorkflowDto.cs`
  - [ ] `NotificationDto.cs` + `CreateNotificationDto.cs` + `UpdateNotificationDto.cs`
- [ ] **Create Interfaces for missing services** (in Application.Contracts):
  - [ ] `IControlAssessmentAppService.cs`
  - [ ] `IRegulatoryFrameworkAppService.cs`
  - [ ] `IRegulatorAppService.cs`
  - [ ] `IVendorAppService.cs`
  - [ ] `IComplianceCalendarAppService.cs`
  - [ ] `IWorkflowAppService.cs`
  - [ ] `INotificationAppService.cs`

## 🟡 HIGH PRIORITY (Core Functionality)

### 5. Blazor UI Pages (13+ pages missing)
- [ ] **Home Page**: `Pages/Home/Index.razor`
- [ ] **Dashboard**: `Pages/Dashboard/Index.razor`
- [ ] **Frameworks**:
  - [ ] `Pages/Frameworks/Index.razor`
  - [ ] `Pages/Frameworks/Create.razor`
  - [ ] `Pages/Frameworks/Edit.razor`
- [ ] **Regulators**: `Pages/Regulators/Index.razor` + Create/Edit
- [ ] **Assessments**:
  - [ ] `Pages/Assessments/Index.razor`
  - [ ] `Pages/Assessments/Create.razor`
  - [ ] `Pages/Assessments/Edit.razor`
- [ ] **Control Assessments**: `Pages/ControlAssessments/Index.razor` + Create/Edit
- [ ] **Risks**:
  - [ ] `Pages/Risks/Index.razor`
  - [ ] `Pages/Risks/Create.razor`
  - [ ] `Pages/Risks/Edit.razor`
- [ ] **Audits**: `Pages/Audits/Index.razor` + Create/Edit
- [ ] **Action Plans**: `Pages/ActionPlans/Index.razor` + Create/Edit
- [ ] **Policies**: `Pages/Policies/Index.razor` + Create/Edit
- [ ] **Compliance Calendar**: `Pages/ComplianceCalendar/Index.razor`
- [ ] **Workflow**: `Pages/Workflow/Index.razor`
- [ ] **Notifications**: `Pages/Notifications/Index.razor`
- [ ] **Vendors**: `Pages/Vendors/Index.razor` + Create/Edit
- [ ] **Reports**: `Pages/Reports/Index.razor`
- [ ] **Integrations**: `Pages/Integrations/Index.razor`
- [ ] **Subscriptions**: `Pages/Subscriptions/Index.razor`

### 6. AutoMapper Configuration
- [ ] **Add mappings for all new entities** in `GrcApplicationAutoMapperProfile.cs`:
  - [ ] ControlAssessment mappings
  - [ ] RegulatoryFramework mappings
  - [ ] Regulator mappings
  - [ ] Vendor mappings
  - [ ] ComplianceEvent mappings
  - [ ] Workflow mappings
  - [ ] Notification mappings

### 7. Fix TODO Comments in Code
- [ ] **SubscriptionAppService.cs** - Remove NotImplementedException, implement properly
- [ ] **AdminAppService.cs** - Implement subscription count logic
- [ ] **TenantManagementAppService.cs** - Implement user count for tenant
- [ ] **AdminApplicationAutoMapperProfile.cs** - Map LastLoginTime properly
- [ ] **Blazor Pages** - Replace all `// TODO: Handle error properly` with proper error handling
- [ ] **Blazor Pages** - Add confirmation dialogs for delete operations
- [ ] **GrcPermissionDefinitionProvider.cs** - Replace TODO with actual localization

## 🟠 MEDIUM PRIORITY (Quality & Completeness)

### 8. Error Handling & Validation
- [ ] **Add Validation Attributes** to all DTOs (Data Annotations or FluentValidation)
- [ ] **Create FluentValidation Validators** for all Create/Update DTOs
- [ ] **Global Exception Handler** - Middleware for error handling
- [ ] **Error Dialog Component** - Reusable error display component
- [ ] **Replace Console.WriteLine** with proper error handling in Blazor pages
- [ ] **Add Confirmation Dialogs** - For all delete operations

### 9. Configuration Files
- [ ] **appsettings.Development.json** - Development environment config
- [ ] **appsettings.Production.json** - Production environment config
- [ ] **appsettings.Staging.json** - Staging environment config
- [ ] **Directory.Build.props** - Solution-level build properties
- [ ] **.editorconfig** - Code style configuration
- [ ] **Update appsettings.json** - Add connection strings, logging, etc.

### 10. Localization
- [ ] **Complete GrcResource.cs** - Add all missing translation keys
- [ ] **Create Resource Files** (.resx or .json) for Arabic and English
- [ ] **Add Translation Keys** for all UI text
- [ ] **Date/Time Localization** - Culture-specific formatting
- [ ] **Number Formatting** - Culture-specific number formatting

### 11. Testing
- [ ] **Create Test Projects**:
  - [ ] `Grc.Application.Tests` project
  - [ ] `Grc.Domain.Tests` project
  - [ ] `Grc.EntityFrameworkCore.Tests` project
- [ ] **Unit Tests**:
  - [ ] `PolicyEnforcerTests.cs`
  - [ ] `DotPathResolverTests.cs`
  - [ ] `MutationApplierTests.cs`
  - [ ] `PolicyStoreTests.cs`
  - [ ] `EvidenceAppServiceTests.cs`
  - [ ] Tests for all other AppServices
- [ ] **Integration Tests**:
  - [ ] `DbContextTests.cs`
  - [ ] `RepositoryIntegrationTests.cs`
  - [ ] `PolicyEnforcementIntegrationTests.cs`

## 🟢 LOW PRIORITY (Nice to Have)

### 12. Documentation
- [ ] **API Documentation** - Swagger/OpenAPI setup
- [ ] **Architecture Documentation** - System design docs
- [ ] **Database Schema Documentation** - ER diagrams
- [ ] **User Guide** - Arabic + English
- [ ] **Admin Guide** - Administration manual
- [ ] **Developer Onboarding Guide** - Setup instructions
- [ ] **API Reference** - Complete API documentation

### 13. Deployment & DevOps
- [ ] **Dockerfile** - Container image definition
- [ ] **docker-compose.yml** - Multi-container setup
- [ ] **Kubernetes Manifests** - K8s deployment configs
- [ ] **CI/CD Pipeline** - GitHub Actions or Azure DevOps
- [ ] **Environment Variables Documentation** - Required env vars
- [ ] **Deployment Runbook** - Step-by-step deployment guide
- [ ] **Rollback Procedures** - How to rollback deployments

### 14. Monitoring & Logging
- [ ] **Structured Logging** - Serilog or NLog setup
- [ ] **Log Aggregation** - ELK stack or Seq configuration
- [ ] **Application Performance Monitoring** - APM tool integration
- [ ] **Health Check Endpoints** - Health check implementation
- [ ] **Metrics Collection** - Prometheus integration
- [ ] **Distributed Tracing** - OpenTelemetry setup

### 15. Additional Features
- [ ] **Search Functionality** - Global search across entities
- [ ] **Bulk Operations** - Bulk delete, bulk update
- [ ] **Export Functionality** - Export to Excel, PDF, CSV
- [ ] **Import Functionality** - Import from Excel, CSV
- [ ] **Advanced Filtering** - Complex filter UI
- [ ] **Sorting** - Multi-column sorting
- [ ] **Pagination** - Improved pagination UI
- [ ] **File Upload** - For evidence attachments
- [ ] **Email Notifications** - Email service integration
- [ ] **Audit Trail** - Complete audit logging

## 📊 Summary

### By Priority:
- **🔴 CRITICAL**: 4 categories, ~40+ tasks
- **🟡 HIGH**: 3 categories, ~30+ tasks
- **🟠 MEDIUM**: 4 categories, ~25+ tasks
- **🟢 LOW**: 4 categories, ~20+ tasks

### Total Estimated Tasks: **115+ tasks**

### Estimated Time:
- **Critical**: 2-3 weeks
- **High Priority**: 2-3 weeks
- **Medium Priority**: 2-3 weeks
- **Low Priority**: Ongoing

**Total Estimated Time to Production-Ready: 6-9 weeks**
