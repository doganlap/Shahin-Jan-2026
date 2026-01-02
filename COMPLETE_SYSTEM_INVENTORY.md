# GRC System - Complete Inventory

## 📋 Table of Contents
1. [Architecture Layers](#architecture-layers)
2. [Domain Entities](#domain-entities)
3. [Application Services & Endpoints](#application-services--endpoints)
4. [Blazor UI Pages](#blazor-ui-pages)
5. [Permissions System](#permissions-system)
6. [Role Profiles](#role-profiles)
7. [Policy Engine](#policy-engine)
8. [Infrastructure Components](#infrastructure-components)
9. [Configuration Files](#configuration-files)

---

## 🏗️ Architecture Layers

### 1. **Domain Layer** (`Grc.Domain`)
- **Purpose**: Core business entities, repositories, domain logic
- **Location**: `src/Grc.Domain/`
- **Key Components**:
  - 14 Entity classes (Evidence, Assessment, Audit, Risk, etc.)
  - 14 Repository interfaces
  - 2 Data Seed Contributors
  - Domain Module (`GrcDomainModule`)

### 2. **Domain.Shared Layer** (`Grc.Domain.Shared`)
- **Purpose**: Shared constants, permissions, role definitions
- **Location**: `src/Grc.Domain.Shared/`
- **Key Components**:
  - `GrcPermissions` (all permission constants)
  - `GrcRoleDefinitions` (8 predefined role profiles)
  - `IGovernedResource` interface
  - Localization resources

### 3. **Application.Contracts Layer** (`Grc.Application.Contracts`)
- **Purpose**: DTOs, interfaces, contracts
- **Location**: `src/Grc.Application.Contracts/`
- **Key Components**:
  - 38+ DTO files
  - 14 AppService interfaces
  - Error handling DTOs
  - Role profile DTOs

### 4. **Application Layer** (`Grc.Application`)
- **Purpose**: Business logic, service implementations
- **Location**: `src/Grc.Application/`
- **Key Components**:
  - 20 AppService implementations
  - Policy Engine (12 files)
  - AutoMapper profiles
  - Application Module

### 5. **Blazor UI Layer** (`Grc.Blazor`)
- **Purpose**: User interface, pages, components
- **Location**: `src/Grc.Blazor/`
- **Key Components**:
  - 50 Razor pages
  - 2 Reusable components
  - Menu configuration
  - Error handling services

### 6. **EntityFrameworkCore Layer** (`Grc.EntityFrameworkCore`)
- **Purpose**: Database context, migrations, EF Core integration
- **Location**: `src/Grc.EntityFrameworkCore/`
- **Key Components**:
  - `GrcDbContext`
  - Entity configurations
  - Migration support

---

## 📦 Domain Entities (14 Entities)

| Entity | Repository Interface | Description |
|--------|---------------------|-------------|
| **Evidence** | `IEvidenceRepository` | Document evidence for compliance |
| **Assessment** | `IAssessmentRepository` | Compliance assessments |
| **Audit** | `IAuditRepository` | Audit records and findings |
| **Risk** | `IRiskRepository` | Risk management records |
| **ActionPlan** | `IActionPlanRepository` | Remediation action plans |
| **PolicyDocument** | `IPolicyDocumentRepository` | Policy documents and versions |
| **ControlAssessment** | `IControlAssessmentRepository` | Control effectiveness assessments |
| **RegulatoryFramework** | `IRegulatoryFrameworkRepository` | Regulatory frameworks (ISO, NIST, etc.) |
| **Regulator** | `IRegulatorRepository` | Regulatory bodies |
| **Vendor** | `IVendorRepository` | Third-party vendor management |
| **ComplianceEvent** | `IComplianceEventRepository` | Compliance calendar events |
| **Workflow** | `IWorkflowRepository` | Workflow definitions and instances |
| **Notification** | `INotificationRepository` | System notifications |
| **Subscription** | `ISubscriptionRepository` | Tenant subscriptions |

---

## 🔌 Application Services & Endpoints

### **Total: 20 AppServices with 85+ Endpoints**

#### 1. **EvidenceAppService** (`IEvidenceAppService`)
- `GetAsync(Guid id)` - Get single evidence
- `GetListAsync(EvidenceListInputDto)` - List with pagination/filtering
- `CreateAsync(CreateEvidenceDto)` - Create new evidence
- `UpdateAsync(Guid id, UpdateEvidenceDto)` - Update evidence
- `DeleteAsync(Guid id)` - Delete evidence
- `ApproveAsync(Guid id)` - Approve evidence
- `SubmitAsync(Guid id)` - Submit for review

#### 2. **AssessmentAppService** (`IAssessmentAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(AssessmentListInputDto)`
- `CreateAsync(CreateAssessmentDto)`
- `UpdateAsync(Guid id, UpdateAssessmentDto)`
- `DeleteAsync(Guid id)`
- `SubmitAsync(Guid id)` - Submit assessment
- `ApproveAsync(Guid id)` - Approve assessment

#### 3. **AuditAppService** (`IAuditAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(AuditListInputDto)`
- `CreateAsync(CreateAuditDto)`
- `UpdateAsync(Guid id, UpdateAuditDto)`
- `DeleteAsync(Guid id)`
- `CloseAsync(Guid id)` - Close audit

#### 4. **RiskAppService** (`IRiskAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(RiskListInputDto)`
- `CreateAsync(CreateRiskDto)`
- `UpdateAsync(Guid id, UpdateRiskDto)`
- `DeleteAsync(Guid id)`
- `AcceptAsync(Guid id)` - Accept risk

#### 5. **ActionPlanAppService** (`IActionPlanAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(ActionPlanListInputDto)`
- `CreateAsync(CreateActionPlanDto)`
- `UpdateAsync(Guid id, UpdateActionPlanDto)`
- `DeleteAsync(Guid id)`
- `AssignAsync(Guid id, Guid userId)` - Assign to user
- `CloseAsync(Guid id)` - Close action plan

#### 6. **PolicyDocumentAppService** (`IPolicyDocumentAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(PolicyDocumentListInputDto)`
- `CreateAsync(CreatePolicyDocumentDto)`
- `UpdateAsync(Guid id, UpdatePolicyDocumentDto)`
- `DeleteAsync(Guid id)`
- `ApproveAsync(Guid id)` - Approve policy
- `PublishAsync(Guid id)` - Publish policy

#### 7. **ControlAssessmentAppService** (`IControlAssessmentAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(ControlAssessmentListInputDto)`
- `CreateAsync(CreateControlAssessmentDto)`
- `UpdateAsync(Guid id, UpdateControlAssessmentDto)`
- `DeleteAsync(Guid id)`

#### 8. **RegulatoryFrameworkAppService** (`IRegulatoryFrameworkAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(RegulatoryFrameworkListInputDto)`
- `CreateAsync(CreateRegulatoryFrameworkDto)`
- `UpdateAsync(Guid id, UpdateRegulatoryFrameworkDto)`
- `DeleteAsync(Guid id)`
- `ImportAsync(ImportFrameworkDto)` - Import framework

#### 9. **RegulatorAppService** (`IRegulatorAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(RegulatorListInputDto)`
- `CreateAsync(CreateRegulatorDto)`
- `UpdateAsync(Guid id, UpdateRegulatorDto)`
- `DeleteAsync(Guid id)`

#### 10. **VendorAppService** (`IVendorAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(VendorListInputDto)`
- `CreateAsync(CreateVendorDto)`
- `UpdateAsync(Guid id, UpdateVendorDto)`
- `DeleteAsync(Guid id)`
- `AssessAsync(Guid id, VendorAssessmentDto)` - Assess vendor

#### 11. **ComplianceCalendarAppService** (`IComplianceCalendarAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(ComplianceEventListInputDto)`
- `CreateAsync(CreateComplianceEventDto)`
- `UpdateAsync(Guid id, UpdateComplianceEventDto)`
- `DeleteAsync(Guid id)`
- `GetUpcomingAsync(int days)` - Get upcoming events
- `GetOverdueAsync()` - Get overdue events

#### 12. **WorkflowAppService** (`IWorkflowAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(WorkflowListInputDto)`
- `CreateAsync(CreateWorkflowDto)`
- `UpdateAsync(Guid id, UpdateWorkflowDto)`
- `DeleteAsync(Guid id)`
- `ExecuteAsync(Guid id, WorkflowExecutionDto)` - Execute workflow

#### 13. **NotificationAppService** (`INotificationAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(NotificationListInputDto)`
- `CreateAsync(CreateNotificationDto)`
- `MarkAsReadAsync(Guid id)` - Mark as read
- `MarkAllAsReadAsync()` - Mark all as read

#### 14. **SubscriptionAppService** (`ISubscriptionAppService`)
- `GetAsync(Guid id)`
- `GetListAsync()`
- `ActivateAsync(Guid id)` - Activate subscription
- `DeactivateAsync(Guid id)` - Deactivate subscription

#### 15. **AdminAppService** (`IAdminAppService`)
- `GetDashboardAsync()` - Get admin dashboard data

#### 16. **UserManagementAppService** (`IUserManagementAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(UserListInputDto)`
- `CreateAsync(CreateUserDto)`
- `UpdateAsync(Guid id, UpdateUserDto)`
- `DeleteAsync(Guid id)`
- `AssignRoleAsync(Guid userId, Guid roleId)` - Assign role
- `RemoveRoleAsync(Guid userId, Guid roleId)` - Remove role

#### 17. **RoleManagementAppService** (`IRoleManagementAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(RoleListInputDto)`
- `CreateAsync(CreateRoleDto)`
- `UpdateAsync(Guid id, UpdateRoleDto)`
- `DeleteAsync(Guid id)`
- `GrantPermissionAsync(Guid roleId, string permission)` - Grant permission
- `RevokePermissionAsync(Guid roleId, string permission)` - Revoke permission

#### 18. **TenantManagementAppService** (`ITenantManagementAppService`)
- `GetAsync(Guid id)`
- `GetListAsync(TenantListInputDto)`
- `CreateAsync(CreateTenantDto)`
- `UpdateAsync(Guid id, UpdateTenantDto)`
- `DeleteAsync(Guid id)`

#### 19. **RoleProfileAppService** (`IRoleProfileAppService`)
- `GetAllProfilesAsync()` - Get all role profiles
- `GetProfileAsync(string roleName)` - Get specific profile
- `GetAvailableProfilesAsync()` - Get available profiles
- `GetProfileSummariesAsync()` - Get profile summaries
- `CreateRoleFromProfileAsync(CreateRoleFromProfileDto)` - Create role from profile

#### 20. **RoleProfileIntegrationService** (`IRoleProfileIntegrationService`)
- `ValidateRoleForModuleAsync(string roleName, string moduleName)` - Validate role
- `GetRecommendedProfilesForModuleAsync(string moduleName)` - Get recommendations
- `CanPerformActionAsync(string roleName, string permission)` - Check permission

---

## 🎨 Blazor UI Pages (50 Pages)

### **Home & Dashboard**
- `Home/Index.razor` - Home page
- `Dashboard/Index.razor` - Dashboard with statistics

### **Core GRC Modules (11 entities × 3 pages = 33 pages)**
Each entity has:
- `{Entity}/Index.razor` - List view with pagination/filtering
- `{Entity}/Create.razor` - Create form
- `{Entity}/Edit.razor` - Edit form

**Entities:**
1. Evidence
2. Assessment
3. Audit
4. Risk
5. ActionPlan
6. PolicyDocument
7. ControlAssessment
8. RegulatoryFramework
9. Regulator
10. Vendor
11. ComplianceCalendar

### **Special Pages**
- `Workflow/Index.razor` - Workflow management
- `Notification/Index.razor` - Notifications list
- `Subscriptions/Index.razor` - Subscription management
- `Reports/Index.razor` - Reports and analytics
- `Integrations/Index.razor` - Integration management

### **Admin Pages (10 pages)**
- `Admin/Index.razor` - Admin dashboard
- `Admin/Users/Index.razor` - User list
- `Admin/Users/Create.razor` - Create user
- `Admin/Users/Edit.razor` - Edit user
- `Admin/Roles/Index.razor` - Role list
- `Admin/Roles/Create.razor` - Create role
- `Admin/Roles/Edit.razor` - Edit role
- `Admin/Roles/Profiles.razor` - Role profiles (NEW)
- `Admin/Tenants/Index.razor` - Tenant list
- `Admin/Tenants/Details.razor` - Tenant details

### **Reusable Components**
- `Components/ErrorDialog.razor` - Error display modal
- `Components/...` - Other shared components

---

## 🔐 Permissions System

### **Permission Groups** (19 Groups)

1. **Home** - `Grc.Home`
2. **Dashboard** - `Grc.Dashboard`
3. **Subscriptions** - `Grc.Subscriptions.*`
   - View, Manage
4. **Admin** - `Grc.Admin.*`
   - Access, Users, Roles, Tenants
5. **Frameworks** - `Grc.Frameworks.*`
   - View, Create, Update, Delete, Import
6. **Regulators** - `Grc.Regulators.*`
   - View, Manage
7. **Assessments** - `Grc.Assessments.*`
   - View, Create, Update, Submit, Approve
8. **ControlAssessments** - `Grc.ControlAssessments.*`
   - View, Manage
9. **Evidence** - `Grc.Evidence.*`
   - View, Upload, Update, Delete, Approve
10. **Risks** - `Grc.Risks.*`
    - View, Manage, Accept
11. **Audits** - `Grc.Audits.*`
    - View, Manage, Close
12. **ActionPlans** - `Grc.ActionPlans.*`
    - View, Manage, Assign, Close
13. **Policies** - `Grc.Policies.*`
    - View, Manage, Approve, Publish
14. **ComplianceCalendar** - `Grc.ComplianceCalendar.*`
    - View, Manage
15. **Workflow** - `Grc.Workflow.*`
    - View, Manage
16. **Notifications** - `Grc.Notifications.*`
    - View, Manage
17. **Vendors** - `Grc.Vendors.*`
    - View, Manage, Assess
18. **Reports** - `Grc.Reports.*`
    - View, Export
19. **Integrations** - `Grc.Integrations.*`
    - View, Manage

**Total: 50+ individual permissions**

---

## 👥 Role Profiles (8 Predefined Roles)

### 1. **SuperAdmin**
- **Display Name**: مدير النظام العام
- **SLA**: 24/7 Support | Response Time: Immediate | Access: Full System
- **Permissions**: `Grc.*` (all permissions)

### 2. **TenantAdmin**
- **Display Name**: مدير العميل
- **SLA**: Business Hours Support | Response Time: 4 hours | Access: Tenant Scope
- **Permissions**: Admin, Subscriptions, Integrations, Users, Roles (within tenant)

### 3. **ComplianceManager**
- **Display Name**: مدير الامتثال
- **SLA**: Business Hours Support | Response Time: 8 hours | Access: Compliance Modules
- **Permissions**: Frameworks, Regulators, Assessments, ControlAssessments, Evidence, Policies, Calendar, Workflow, Reports

### 4. **RiskManager**
- **Display Name**: مدير المخاطر
- **SLA**: Business Hours Support | Response Time: 8 hours | Access: Risk Modules
- **Permissions**: Risks, ActionPlans, Reports

### 5. **Auditor**
- **Display Name**: مراجع
- **SLA**: Business Hours Support | Response Time: 24 hours | Access: Read-Only Audit
- **Permissions**: Audits (read-only), Evidence (read-only), Assessments (read-only)

### 6. **EvidenceOfficer**
- **Display Name**: مسؤول الأدلة
- **SLA**: Business Hours Support | Response Time: 24 hours | Access: Evidence Management
- **Permissions**: Evidence (Upload, Update, Submit)

### 7. **VendorManager**
- **Display Name**: مدير الموردين
- **SLA**: Business Hours Support | Response Time: 24 hours | Access: Vendor Management
- **Permissions**: Vendors, Vendor Assessments

### 8. **Viewer**
- **Display Name**: مستعرض
- **SLA**: Business Hours Support | Response Time: 48 hours | Access: Read-Only
- **Permissions**: View-only on all modules (no Export)

---

## ⚙️ Policy Engine

### **Location**: `src/Grc.Application/Policy/`

### **Core Components** (12 files):

1. **PolicyContext.cs** - Context for policy evaluation
2. **IPolicyEnforcer.cs** - Policy enforcer interface
3. **PolicyEnforcer.cs** - Policy enforcement implementation
4. **PolicyStore.cs** - Policy loading and caching
5. **DotPathResolver.cs** - Dot-path value resolution
6. **MutationApplier.cs** - Resource mutation logic
7. **PolicyViolationException.cs** - Custom exception
8. **PolicyAuditLogger.cs** - Audit logging
9. **BasePolicyAppService.cs** - Base class for AppServices
10. **IEnvironmentProvider.cs** - Environment detection
11. **EnvironmentProvider.cs** - Environment implementation
12. **IRoleResolver.cs** / **RoleResolver.cs** - Role resolution

### **Policy Models**:
- `PolicyModels/PolicyDocument.cs`
- `PolicyModels/PolicyRule.cs`

### **Features**:
- Deterministic rule evaluation
- YAML policy file support
- Exception handling
- Mutation support
- Audit logging
- Multi-tenant safe

---

## 🛠️ Infrastructure Components

### **Data Seed Contributors**
1. **GrcRoleDataSeedContributor** (`Grc.Domain/Seed/`)
   - Creates 8 predefined roles
   - Grants permissions automatically
   - Transactional seeding
   - Comprehensive logging

2. **GrcAdminUserDataSeedContributor** (`Grc.Domain/Seed/`)
   - Creates default admin user

### **Services**
1. **ErrorToastService** (`Grc.Blazor/Services/`)
   - Error notification service
   - Success notifications
   - Browser console integration

### **Menu System**
- **GrcMenuContributor** (`Grc.Blazor/Menus/`)
  - Arabic menu items
  - Permission-based visibility
  - 19 main menu items
  - Sub-menus for Admin section

### **AutoMapper Profiles**
1. **GrcApplicationAutoMapperProfile** - Main mappings
2. **AdminApplicationAutoMapperProfile** - Admin mappings

### **Modules**
1. **GrcDomainModule** - Domain module
2. **GrcDomainSharedModule** - Shared module
3. **GrcApplicationModule** - Application module
4. **GrcBlazorModule** - Blazor module

---

## 📄 Configuration Files

### **Application Configuration**
- `appsettings.json` - Main configuration
- `appsettings.Development.json` - Development settings
- `appsettings.Staging.json` - Staging settings
- `appsettings.Production.json` - Production settings

### **Project Files**
- `Grc.Domain.csproj`
- `Grc.Domain.Shared.csproj`
- `Grc.Application.csproj`
- `Grc.Application.Contracts.csproj`
- `Grc.Blazor.csproj`
- `Grc.EntityFrameworkCore.csproj`

### **Solution File**
- `Grc.sln` - Visual Studio solution

---

## 📊 Statistics Summary

| Category | Count |
|----------|-------|
| **Domain Entities** | 14 |
| **Repository Interfaces** | 14 |
| **Application Services** | 20 |
| **API Endpoints** | 85+ |
| **DTOs** | 38+ |
| **Blazor Pages** | 50 |
| **Permission Groups** | 19 |
| **Individual Permissions** | 50+ |
| **Role Profiles** | 8 |
| **Policy Engine Files** | 12 |
| **Data Seed Contributors** | 2 |
| **AutoMapper Profiles** | 2 |
| **ABP Modules** | 4 |

---

## ✅ Production Readiness Status

### **Completed ✅**
- ✅ All domain entities and repositories
- ✅ All application services and endpoints
- ✅ All DTOs and contracts
- ✅ All Blazor UI pages
- ✅ Permission system
- ✅ Role profiles system
- ✅ Policy engine infrastructure
- ✅ Error handling
- ✅ Menu system
- ✅ Data seeding

### **Infrastructure Pending ⚠️**
- ⚠️ NuGet package resolution (ABP commercial packages)
- ⚠️ Database migrations (requires NuGet resolution)
- ⚠️ Manual testing and validation

---

## 📝 Notes

- All code follows ABP Framework best practices
- Multi-tenant architecture supported
- Arabic UI with RTL support
- Comprehensive error handling
- Logging throughout all layers
- Type-safe permissions using constants
- Deterministic policy evaluation
- Production-ready code quality

---

**Last Updated**: 2025-01-22
**System Version**: 1.0.0
**Status**: Code Complete, Awaiting Infrastructure Setup
