# GRC System - Predefined Role Profiles

## Overview

This document describes all predefined role profiles in the GRC system, including their descriptions, permissions, and SLA definitions.

---

## Role Profiles

### 1. SuperAdmin (مدير النظام العام)

**Description**: صلاحيات كاملة على جميع وحدات النظام. يمكنه إدارة المستخدمين، الأدوار، العملاء، وجميع البيانات.

**SLA**: 24/7 Support | Response Time: Immediate | Access: Full System

**Permissions**: 
- All GRC permissions (`Grc.*`)

**Use Case**: System administrators who need complete control over the entire GRC system.

---

### 2. TenantAdmin (مدير العميل)

**Description**: إدارة العميل الكاملة: إدارة المستخدمين والأدوار داخل العميل، الاشتراكات، التكاملات، وإعدادات العميل.

**SLA**: Business Hours Support | Response Time: 4 hours | Access: Tenant Scope

**Permissions**:
- Admin: Access, Users, Roles
- Subscriptions: View, Manage
- Integrations: View, Manage
- View-only access to all GRC modules

**Use Case**: Tenant administrators managing their organization's users, roles, subscriptions, and integrations.

---

### 3. ComplianceManager (مدير الامتثال)

**Description**: إدارة شاملة لعمليات الامتثال: الأطر التنظيمية، الجهات التنظيمية، التقييمات، الأدلة، السياسات، تقويم الامتثال، سير العمل، والتقارير.

**SLA**: Business Hours Support | Response Time: 8 hours | Access: Compliance Modules

**Permissions**:
- Frameworks: Full CRUD + Import
- Regulators: View, Manage
- Assessments: View, Create, Update, Submit, Approve
- Control Assessments: View, Manage
- Evidence: View, Upload, Update, Delete, Approve
- Policies: View, Manage, Approve, Publish
- Compliance Calendar: View, Manage
- Workflow: View, Manage
- Reports: View, Export

**Use Case**: Compliance officers managing all compliance-related activities and ensuring regulatory adherence.

---

### 4. RiskManager (مدير المخاطر)

**Description**: إدارة شاملة للمخاطر: تحديد المخاطر، تقييمها، قبولها، وربطها بخطط العمل. الوصول إلى التقارير المتعلقة بالمخاطر.

**SLA**: Business Hours Support | Response Time: 8 hours | Access: Risk & Action Plan Modules

**Permissions**:
- Risks: View, Manage, Accept
- Action Plans: View, Manage, Assign, Close
- Reports: View, Export
- Assessments: View (read-only)
- Evidence: View (read-only)

**Use Case**: Risk management professionals identifying, assessing, and mitigating organizational risks.

---

### 5. Auditor (مراجع)

**Description**: إدارة المراجعات وإغلاقها. الوصول للقراءة فقط على الأدلة والتقييمات للمراجعة.

**SLA**: Business Hours Support | Response Time: 24 hours | Access: Audit Module + Read-Only Evidence/Assessments

**Permissions**:
- Audits: View, Manage, Close
- Evidence: View (read-only)
- Assessments: View (read-only)

**Use Case**: Internal or external auditors conducting audits and reviewing evidence and assessments.

---

### 6. EvidenceOfficer (مسؤول الأدلة)

**Description**: رفع وتحديث الأدلة وتقديمها للمراجعة. لا يمكنه اعتماد الأدلة.

**SLA**: Business Hours Support | Response Time: 24 hours | Access: Evidence Upload/Update/Submit

**Permissions**:
- Evidence: View, Upload, Update, Delete
- Note: Cannot approve evidence

**Use Case**: Staff members responsible for uploading and managing evidence documents.

---

### 7. VendorManager (مدير الموردين)

**Description**: إدارة الموردين وتقييمهم. إدارة تقييمات الموردين وتحديث تصنيفات المخاطر.

**SLA**: Business Hours Support | Response Time: 24 hours | Access: Vendor Management

**Permissions**:
- Vendors: View, Manage, Assess

**Use Case**: Procurement or vendor management teams assessing and managing vendor relationships.

---

### 8. Viewer (مشاهد)

**Description**: الوصول للقراءة فقط على جميع وحدات النظام. لا يمكنه إنشاء أو تعديل أو حذف أي بيانات.

**SLA**: Business Hours Support | Response Time: 48 hours | Access: Read-Only All Modules

**Permissions**:
- View-only access to all GRC modules
- No create, update, delete, or export permissions

**Use Case**: Stakeholders who need to view GRC data but should not modify it.

---

### 9. ComplianceOfficer (ضابط الامتثال)

**Description**: إدارة تقويم الامتثال والأحداث. إنشاء وتحديث أحداث الامتثال ومتابعتها.

**SLA**: Business Hours Support | Response Time: 24 hours | Access: Compliance Calendar Management

**Permissions**:
- Compliance Calendar: View, Manage
- Frameworks: View (read-only)
- Regulators: View (read-only)

**Use Case**: Compliance officers managing compliance calendar events and deadlines.

---

### 10. PolicyManager (مدير السياسات)

**Description**: إدارة السياسات: إنشاء، تحديث، اعتماد، ونشر السياسات. إدارة دورة حياة السياسات بالكامل.

**SLA**: Business Hours Support | Response Time: 8 hours | Access: Policy Management

**Permissions**:
- Policies: View, Manage, Approve, Publish

**Use Case**: Policy administrators managing organizational policies through their lifecycle.

---

### 11. WorkflowAdministrator (مدير سير العمل)

**Description**: إدارة محرك سير العمل: إنشاء، تحديث، تنفيذ، ومراقبة سير العمل. إدارة تعريفات سير العمل.

**SLA**: Business Hours Support | Response Time: 8 hours | Access: Workflow Management

**Permissions**:
- Workflow: View, Manage

**Use Case**: System administrators managing workflow definitions and execution.

---

## Role Hierarchy

```
SuperAdmin (Full Access)
├── TenantAdmin (Tenant Scope)
│   ├── ComplianceManager (Compliance Modules)
│   │   ├── ComplianceOfficer (Calendar Only)
│   │   ├── PolicyManager (Policies Only)
│   │   └── EvidenceOfficer (Evidence Upload Only)
│   ├── RiskManager (Risk & Action Plans)
│   ├── VendorManager (Vendors)
│   ├── WorkflowAdministrator (Workflows)
│   └── Auditor (Audits + Read-Only)
└── Viewer (Read-Only All)
```

---

## Permission Matrix

| Module | SuperAdmin | TenantAdmin | ComplianceManager | RiskManager | Auditor | EvidenceOfficer | VendorManager | Viewer | ComplianceOfficer | PolicyManager | WorkflowAdmin |
|--------|-----------|-------------|-------------------|-------------|---------|-----------------|--------------|--------|-------------------|---------------|---------------|
| Home | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Admin | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Subscriptions | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Frameworks | ✅ | 👁️ | ✅ | ❌ | ❌ | ❌ | ❌ | 👁️ | 👁️ | ❌ | ❌ |
| Regulators | ✅ | 👁️ | ✅ | ❌ | ❌ | ❌ | ❌ | 👁️ | 👁️ | ❌ | ❌ |
| Assessments | ✅ | 👁️ | ✅ | 👁️ | 👁️ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Control Assessments | ✅ | 👁️ | ✅ | ❌ | ❌ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Evidence | ✅ | 👁️ | ✅ | 👁️ | 👁️ | ✅* | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Risks | ✅ | 👁️ | ❌ | ✅ | ❌ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Audits | ✅ | 👁️ | ❌ | ❌ | ✅ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Action Plans | ✅ | 👁️ | ❌ | ✅ | ❌ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Policies | ✅ | 👁️ | ✅ | ❌ | ❌ | ❌ | ❌ | 👁️ | ❌ | ✅ | ❌ |
| Compliance Calendar | ✅ | 👁️ | ✅ | ❌ | ❌ | ❌ | ❌ | 👁️ | ✅ | ❌ | ❌ |
| Workflow | ✅ | 👁️ | ✅ | ❌ | ❌ | ❌ | ❌ | 👁️ | ❌ | ❌ | ✅ |
| Notifications | ✅ | 👁️ | ❌ | ❌ | ❌ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Vendors | ✅ | 👁️ | ❌ | ❌ | ❌ | ❌ | ✅ | 👁️ | ❌ | ❌ | ❌ |
| Reports | ✅ | 👁️ | ✅ | ✅ | ❌ | ❌ | ❌ | 👁️ | ❌ | ❌ | ❌ |
| Integrations | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

**Legend**:
- ✅ = Full access (Create, Read, Update, Delete, and business operations)
- 👁️ = Read-only access
- ✅* = Upload/Update/Delete but cannot Approve
- ❌ = No access

---

## Implementation

### Code Location
- **Role Definitions**: `src/Grc.Domain.Shared/Roles/GrcRoleDefinitions.cs`
- **Data Seeding**: `src/Grc.Domain/Seed/GrcRoleDataSeedContributor.cs`
- **API Service**: `src/Grc.Application/Roles/RoleProfileAppService.cs`
- **DTOs**: `src/Grc.Application.Contracts/Roles/RoleProfileDto.cs`

### Usage

Roles are automatically seeded when the application starts. To manually seed roles:

```csharp
// Roles are seeded via IDataSeedContributor
// Run: dotnet ef database update
```

To get role profiles via API:

```csharp
// GET /api/app/role-profile/profiles
// GET /api/app/role-profile/profile/{roleName}
// GET /api/app/role-profile/available
```

---

## SLA Definitions

| Role | Support Hours | Response Time | Access Level |
|------|--------------|---------------|--------------|
| SuperAdmin | 24/7 | Immediate | Full System |
| TenantAdmin | Business Hours | 4 hours | Tenant Scope |
| ComplianceManager | Business Hours | 8 hours | Compliance Modules |
| RiskManager | Business Hours | 8 hours | Risk & Action Plans |
| PolicyManager | Business Hours | 8 hours | Policy Management |
| WorkflowAdministrator | Business Hours | 8 hours | Workflow Management |
| Auditor | Business Hours | 24 hours | Audit Module |
| ComplianceOfficer | Business Hours | 24 hours | Compliance Calendar |
| EvidenceOfficer | Business Hours | 24 hours | Evidence Upload |
| VendorManager | Business Hours | 24 hours | Vendor Management |
| Viewer | Business Hours | 48 hours | Read-Only All |

---

**Last Updated**: $(date)
**Version**: 1.0
