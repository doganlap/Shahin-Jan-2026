# GRC System - Complete Implementation

## Overview

This is a complete GRC (Governance, Risk, Compliance) system built on ABP Framework with Blazor UI, implementing deterministic policy enforcement and Arabic menu navigation.

## Architecture

### Projects Structure

```
grc-system/
├── src/
│   ├── Grc.Domain.Shared/          # Shared domain constants and interfaces
│   ├── Grc.Application.Contracts/   # Application contracts and permission definitions
│   ├── Grc.Application/              # Application services and policy engine
│   ├── Grc.Domain/                   # Domain entities and seed data
│   └── Grc.Blazor/                  # Blazor UI and menu configuration
└── etc/
    └── policies/                     # YAML policy files
```

## Key Components

### 1. Permissions System
- **GrcPermissions.cs**: Centralized permission constants
- **GrcPermissionDefinitionProvider**: ABP permission definitions
- **GrcMenuContributor**: Arabic menu with permission-based visibility

### 2. Policy Engine
- **PolicyContext**: Context for policy evaluation
- **PolicyEnforcer**: Deterministic rule evaluation engine
- **PolicyStore**: YAML policy loader with caching
- **DotPathResolver**: Dot-path value resolution
- **MutationApplier**: Deterministic resource mutations
- **PolicyAuditLogger**: Decision logging

### 3. Integration
- **BasePolicyAppService**: Base class for AppServices with policy enforcement
- **EvidenceAppService**: Example implementation with policy checks

## Features

### Permissions
- Complete permission hierarchy for all GRC modules
- Permission-based menu visibility
- Role-based access control

### Policy Enforcement
- Deterministic rule evaluation (priority-based)
- YAML-based policy configuration
- Support for allow/deny/audit/mutate effects
- Exception handling with expiry
- Conflict resolution strategies

### Menu (Arabic)
- Complete Arabic menu matching specifications
- Permission-protected routes
- Icon support

## Default Roles

1. **SuperAdmin**: All permissions
2. **TenantAdmin**: Admin + Subscriptions + Integrations
3. **ComplianceManager**: Frameworks, Regulators, Assessments, Evidence, Policies
4. **RiskManager**: Risks, Action Plans
5. **Auditor**: Audits + read-only access
6. **EvidenceOfficer**: Evidence upload/update/submit
7. **VendorManager**: Vendor management
8. **Viewer**: Read-only access

## Policy Rules (Baseline)

1. **REQUIRE_DATA_CLASSIFICATION**: All resources must have data classification
2. **REQUIRE_OWNER**: All resources must have an owner
3. **PROD_RESTRICTED_MUST_HAVE_APPROVAL**: Restricted data in prod requires approval
4. **NORMALIZE_EMPTY_LABELS**: Normalize invalid owner values

## Usage Example

```csharp
[Authorize(GrcPermissions.Evidence.Upload)]
public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto input)
{
    var entity = MapToEntity(input);
    
    // Policy enforcement happens here
    await EnforceAsync("create", "Evidence", entity);
    
    await _repository.InsertAsync(entity);
    return MapToDto(entity);
}
```

## Next Steps

1. Install .NET SDK (if not already installed)
2. Create ABP solution using ABP CLI
3. Copy files to appropriate projects
4. Install NuGet packages:
   - YamlDotNet
   - Volo.Abp.* packages
5. Configure dependency injection
6. Run database migrations
7. Seed roles and permissions

## Notes

- Policy evaluation is deterministic (same input = same output)
- All enforcement happens in backend (no UI-only checks)
- Audit logging for all policy decisions
- Multi-tenant safe
