# Role Profiles System - Professional Integration Guide

## Overview

The GRC Role Profiles system provides a professional, integrated solution for managing predefined roles across all layers of the application. This document describes the complete integration architecture.

---

## Architecture Layers

### 1. Domain.Shared Layer (`Grc.Domain.Shared`)

**Purpose**: Centralized role definitions with no dependencies on other layers.

**Files**:
- `Roles/GrcRoleDefinitions.cs` - Static role definitions with permissions using `GrcPermissions` constants

**Key Features**:
- ✅ Uses `GrcPermissions` constants (no magic strings)
- ✅ Type-safe permission references
- ✅ Arabic descriptions and SLA definitions
- ✅ Immutable role definitions

**Example**:
```csharp
public static class ComplianceManager
{
    public const string Name = "ComplianceManager";
    public const string DisplayName = "مدير الامتثال";
    public const string Description = "...";
    public const string SLA = "...";
    
    public static readonly string[] Permissions = new[]
    {
        GrcPermissions.Home.Default,
        GrcPermissions.Frameworks.View,
        GrcPermissions.Frameworks.Create,
        // ... etc
    };
}
```

---

### 2. Domain Layer (`Grc.Domain`)

**Purpose**: Data seeding and role creation logic.

**Files**:
- `Seed/GrcRoleDataSeedContributor.cs` - Seeds roles on database initialization

**Key Features**:
- ✅ Implements `IDataSeedContributor` (auto-discovered by ABP)
- ✅ Multi-tenant aware
- ✅ Transactional with Unit of Work
- ✅ Comprehensive error handling and logging
- ✅ Updates existing roles if definitions change
- ✅ Validates permissions before granting

**Integration Points**:
- Uses `IdentityRoleManager` for role creation
- Uses `IPermissionManager` for permission assignment
- Uses `ICurrentTenant` for tenant isolation
- Uses `IUnitOfWorkManager` for transactions

---

### 3. Application.Contracts Layer (`Grc.Application.Contracts`)

**Purpose**: Service contracts and DTOs.

**Files**:
- `Roles/RoleProfileDto.cs` - Full role profile DTO
- `Roles/RoleProfileSummaryDto.cs` - Lightweight summary DTO
- `Roles/CreateRoleFromProfileDto.cs` - Input DTO for creating roles
- `Roles/IRoleProfileAppService.cs` - Main service interface
- `Roles/IRoleProfileIntegrationService.cs` - Integration service interface

**Key Features**:
- ✅ Clean separation of concerns
- ✅ DTOs for different use cases
- ✅ Integration service for cross-module validation

---

### 4. Application Layer (`Grc.Application`)

**Purpose**: Business logic and service implementations.

**Files**:
- `Roles/RoleProfileAppService.cs` - Main service implementation
- `Roles/RoleProfileIntegrationService.cs` - Integration service implementation

**Key Features**:
- ✅ Authorization via `[Authorize]` attributes
- ✅ Comprehensive logging
- ✅ Error handling with ABP BusinessException
- ✅ Efficient user count calculation
- ✅ Permission validation
- ✅ Role creation from profiles

**Service Registration**:
```csharp
// In GrcApplicationModule.cs
services.AddTransient<IRoleProfileAppService, RoleProfileAppService>();
services.AddTransient<IRoleProfileIntegrationService, RoleProfileIntegrationService>();
```

---

### 5. Blazor Layer (`Grc.Blazor`)

**Purpose**: UI components and pages.

**Files**:
- `Pages/Admin/Roles/Profiles.razor` - Role profiles browser page
- `Menus/GrcMenuContributor.cs` - Menu integration

**Key Features**:
- ✅ Permission-based access control
- ✅ Error handling with ErrorDialog
- ✅ User-friendly Arabic interface
- ✅ Create role from profile functionality
- ✅ Permission viewer dialog

**Menu Integration**:
```csharp
var rolesMenu = new ApplicationMenuItem("Grc.Admin.Roles", "الأدوار", "/admin/roles")
    .RequirePermissions(GrcPermissions.Admin.Roles);
rolesMenu.AddItem(new ApplicationMenuItem("Grc.Admin.Roles.Profiles", 
    "ملفات الأدوار", "/admin/roles/profiles"));
```

---

## Service Integration Points

### 1. Role Management Integration

**Location**: `RoleManagementAppService`

**Integration**:
- Role profiles can be used as templates when creating new roles
- User count calculation uses same logic for consistency
- Permission management aligned with ABP standards

### 2. User Management Integration

**Location**: `UserManagementAppService`

**Integration**:
- Role profiles displayed in role picker
- Validation ensures users get appropriate roles
- Role assignment respects profile definitions

### 3. Policy Enforcement Integration

**Location**: `BasePolicyAppService`, `PolicyEnforcer`

**Integration**:
- Role profiles define permissions used by policy engine
- Policy rules can reference role names
- Role-based access control enforced at API level

---

## API Endpoints

### Role Profile Service

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/app/role-profile/profiles` | Get all role profiles | `Grc.Admin.Roles` |
| GET | `/api/app/role-profile/profile/{roleName}` | Get specific profile | `Grc.Admin.Roles` |
| GET | `/api/app/role-profile/available` | Get available roles | `Grc.Admin.Roles` |
| GET | `/api/app/role-profile/summaries` | Get profile summaries | `Grc.Admin.Roles` |
| POST | `/api/app/role-profile/create-from-profile` | Create role from profile | `Grc.Admin.Roles` |

### Integration Service

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/app/role-profile-integration/validate/{roleName}/{moduleName}` | Validate role for module | `Grc.Admin.Roles` |
| GET | `/api/app/role-profile-integration/recommended/{moduleName}` | Get recommended profiles | `Grc.Admin.Roles` |
| GET | `/api/app/role-profile-integration/can-perform/{roleName}/{permission}` | Check action permission | `Grc.Admin.Roles` |

---

## Data Flow

### Role Seeding Flow

```
Application Start
    ↓
ABP Data Seed Framework
    ↓
GrcRoleDataSeedContributor.SeedAsync()
    ↓
For each role definition:
    ├─ Check if role exists
    ├─ Create role (if new)
    ├─ Remove old GRC permissions (if updating)
    └─ Grant permissions from definition
    ↓
Save to database
```

### Role Profile Retrieval Flow

```
API Request
    ↓
RoleProfileAppService.GetAllProfilesAsync()
    ↓
Get role definitions from GrcRoleDefinitions
    ↓
For each definition:
    ├─ Query database for role
    ├─ Get user count
    └─ Get granted permissions
    ↓
Map to RoleProfileDto
    ↓
Return to client
```

---

## Error Handling

### Seeding Errors

- **Role Creation Failure**: Logged, continues with next role
- **Permission Grant Failure**: Logged, continues with next permission
- **Transaction Failure**: Rolled back, error logged

### Service Errors

- **Role Not Found**: Returns `BusinessException` with clear message
- **Permission Validation**: Returns `false` with warning log
- **Database Errors**: Logged and re-thrown

---

## Logging

All services use `ILogger<T>` for structured logging:

- **Information**: Normal operations (role creation, retrieval)
- **Warning**: Non-critical issues (missing permissions, role not found)
- **Error**: Critical failures (exceptions, validation failures)

**Log Levels**:
- `LogInformation`: Successful operations
- `LogWarning`: Recoverable issues
- `LogError`: Exceptions and failures

---

## Testing Integration

### Unit Tests

```csharp
// Test role definitions
var roles = GrcRoleDefinitions.GetAllRoles();
Assert.Equal(11, roles.Length);
Assert.Contains(roles, r => r.Name == "SuperAdmin");

// Test permission constants
Assert.Contains(GrcRoleDefinitions.ComplianceManager.Permissions, 
    p => p == GrcPermissions.Frameworks.View);
```

### Integration Tests

```csharp
// Test role seeding
await SeedContributor.SeedAsync(context);
var role = await RoleRepository.FindByNameAsync("ComplianceManager");
Assert.NotNull(role);

// Test service retrieval
var profiles = await RoleProfileAppService.GetAllProfilesAsync();
Assert.Equal(11, profiles.Count);
```

---

## Best Practices

### 1. Permission Constants

✅ **DO**: Use `GrcPermissions` constants
```csharp
GrcPermissions.Evidence.View
```

❌ **DON'T**: Use magic strings
```csharp
"Grc.Evidence.View"  // Bad
```

### 2. Error Handling

✅ **DO**: Use ABP BusinessException
```csharp
throw new Volo.Abp.BusinessException("Role not found");
```

❌ **DON'T**: Use generic Exception
```csharp
throw new Exception("Role not found");  // Bad
```

### 3. Logging

✅ **DO**: Log at appropriate levels with context
```csharp
_logger.LogInformation("Created role: {RoleName}", roleName);
```

❌ **DON'T**: Log sensitive information
```csharp
_logger.LogInformation("User password: {Password}", password);  // Bad
```

### 4. Multi-Tenancy

✅ **DO**: Always use tenant context
```csharp
using (_currentTenant.Change(context.TenantId))
{
    // Operations
}
```

---

## Configuration

### appsettings.json

No additional configuration required. Roles are seeded automatically.

### Manual Seeding

To manually trigger role seeding:

```csharp
// Via ABP CLI
abp seed

// Or programmatically
await SeedContributor.SeedAsync(context);
```

---

## Performance Considerations

1. **User Count Calculation**: Uses efficient query with role name lookup
2. **Permission Retrieval**: Cached by ABP permission manager
3. **Role Lookup**: Uses normalized name index for fast retrieval
4. **Batch Operations**: Processes roles sequentially to avoid conflicts

---

## Security Considerations

1. **Authorization**: All endpoints require `Grc.Admin.Roles` permission
2. **Input Validation**: Role names validated against definitions
3. **Permission Validation**: Permissions verified before granting
4. **Tenant Isolation**: Roles scoped to tenant context
5. **Audit Trail**: All role operations logged

---

## Future Enhancements

1. **Role Templates**: Custom role templates based on profiles
2. **Role Hierarchy**: Support for role inheritance
3. **Dynamic Permissions**: Runtime permission updates
4. **Role Analytics**: Usage statistics and recommendations
5. **Export/Import**: Role profile export/import functionality

---

**Last Updated**: $(date)
**Version**: 2.0 (Professional Integration)
