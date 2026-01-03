# CLAUDE.md - Project Instructions for Claude Code

## Project Overview

This is the **GRC System** (Governance, Risk, and Compliance) - an enterprise application built with ABP Framework and Blazor Server, featuring a sophisticated policy enforcement engine and Arabic menu navigation.

## Technology Stack

- **Framework**: ABP Framework 8.3.0
- **UI**: Blazor Server with Arabic menu labels and permission-gated navigation
- **Backend**: .NET 8.0
- **Database**: PostgreSQL (with Entity Framework Core)
- **ORM**: Entity Framework Core
- **Authentication**: ABP Identity with Azure AD integration
- **Caching**: Redis
- **Policy Engine**: Custom deterministic policy enforcement with YAML-based rules
- **Deployment**: Docker, Kubernetes (Azure)

## Project Structure

```
src/
├── Grc.Domain.Shared/        # Shared constants, enums, permissions, role definitions
├── Grc.Domain/               # Domain entities, repositories, seed data
├── Grc.Application.Contracts/ # DTOs, interfaces, permission definitions
├── Grc.Application/          # Application services, policy engine
├── Grc.Blazor/               # Blazor Server UI with Arabic menus
└── Grc.EntityFrameworkCore/  # EF Core DbContext and migrations

etc/
└── policies/                 # YAML policy files (e.g., grc-baseline.yml)
```

## Key Components

### 1. Policy Engine (First-Class Feature)

The policy engine is located in `src/Grc.Application/Policy/` and includes:

- **PolicyEnforcer**: Deterministic rule evaluation engine with priority-based evaluation
- **PolicyStore**: YAML policy loader with caching (explicit invalidation via `InvalidateCache()`)
- **PolicyContext**: Context for policy evaluation (user, tenant, roles, environment)
- **PolicyAuditLogger**: Decision logging for audit trails
- **DotPathResolver**: Dot-path value resolution for policy conditions
- **MutationApplier**: Deterministic resource mutations

**Policy Evaluation Rules:**
- Policies are evaluated in ascending `priority` order
- Short-circuit evaluation on first match
- Deny-overrides conflict resolution
- Supports allow/deny/audit/mutate effects
- Exception handling with expiry dates

### 2. Permissions System

- **Centralized Permissions**: `src/Grc.Domain.Shared/Permissions/GrcPermissions.cs`
- **Permission Definitions**: `GrcPermissionDefinitionProvider` (ABP framework)
- **Menu Integration**: Arabic menu in `src/Grc.Blazor/Menus/GrcMenuContributor.cs`
- Keep ABP `[Authorize]` attributes consistent with permission constants

### 3. Role Profiles

- **Implementation**: `src/Grc.Application/Roles/`
  - `RoleProfileAppService`: Main service for role profiles
  - `RoleProfileIntegrationService`: Integration service
- **Registration**: Transient services in `GrcApplicationModule`
- **Default Roles**: SuperAdmin, TenantAdmin, ComplianceManager, RiskManager, Auditor, EvidenceOfficer, VendorManager, Viewer

### 4. Application Services

- **Base Class**: All AppServices inherit from `BasePolicyAppService`
- **Policy Enforcement**: **Must call** `EnforceAsync(action, resourceType, resource, environment?)` before mutating or returning sensitive data
- **Standard Verbs**: `"create"`, `"update"`, `"delete"`, `"view"`, etc.
- **Resource Types**: Match entity names (e.g., `"Evidence"`, `"Risk"`, `"Assessment"`)

**Example:**
```csharp
[Authorize(GrcPermissions.Evidence.Upload)]
public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto input)
{
    var entity = MapToEntity(input);
    
    // Policy enforcement is mandatory
    await EnforceAsync("create", "Evidence", entity);
    
    await _repository.InsertAsync(entity);
    return MapToDto(entity);
}
```

## Key Conventions

### Naming Conventions
- **Entities**: PascalCase, singular (e.g., `Risk`, `Control`, `Assessment`)
- **DTOs**: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`
- **AppServices**: `{Entity}AppService` implementing `I{Entity}AppService`
- **Repositories**: `I{Entity}Repository`

### Code Style
- Use `async/await` for all I/O operations
- Follow ABP's `ApplicationService` base class patterns
- Use `IRepository<T>` or custom repositories
- Implement `ITransientDependency` or `ISingletonDependency` for DI
- Follow `.editorconfig` and StyleCop conventions

### DTO Mapping
- AutoMapper profiles: `GrcApplicationAutoMapperProfile` and `AdminApplicationAutoMapperProfile`
- Registered in `GrcApplicationModule`
- Add or adjust maps in profiles; avoid inline mapping in services

### Database
- PostgreSQL with EF Core
- Migrations in `Grc.EntityFrameworkCore/Migrations`
- Use `GrcDbContext` for data access

### Localization
- Resources in `Grc.Domain.Shared/Localization/Grc`
- Support for English (en) and Arabic (ar)
- Arabic menu labels in `GrcMenuContributor.cs`

## Build and Tooling

### Building the Project
```bash
# Restore and build
dotnet restore && dotnet build

# Release build
scripts/build.sh
```

### Code Quality
- **Analyzers** (enabled via `Directory.Build.props`):
  - StyleCop
  - .NET Analyzers
  - SecurityCodeScan
  - AsyncFixer
  - ConfigureAwaitChecker
- **Warnings**: Not treated as errors but should be kept clean
- **Format**: `dotnet format --verify-no-changes`

### Running the Application
```bash
cd src/Grc.Blazor
dotnet run
```

### Running Tests
```bash
dotnet test
```

### Building Docker Image
```bash
docker build -t grc-blazor -f src/Grc.Blazor/Dockerfile .
```

## Common Tasks

### Adding a New Entity
1. Create entity in `Grc.Domain/Entities`
2. Create repository interface in `Grc.Domain/Repositories`
3. Add DbSet to `GrcDbContext` (in EntityFrameworkCore project)
4. Create DTOs in `Grc.Application.Contracts`
5. Create AppService in `Grc.Application` (inherit from `BasePolicyAppService`)
6. Add AutoMapper mappings to `GrcApplicationAutoMapperProfile`
7. Add permissions to `GrcPermissions.cs` and `GrcPermissionDefinitionProvider`
8. Create Blazor UI pages in `Grc.Blazor/Pages`
9. Add menu items to `GrcMenuContributor.cs`
10. Add migration: `dotnet ef migrations add <Name>`

### Adding Policy Rules
1. Create or update YAML file in `etc/policies/`
2. Ensure file is copied to output directory
3. Define rules with priority, conditions, and effects
4. Call `PolicyStore.InvalidateCache()` if needed at runtime

### Working with Policies
- **Policy Files**: Store in `etc/policies/` (YAML format)
- **Loading**: Policies are cached on first load
- **Runtime**: Access via `AppContext.BaseDirectory/etc/policies`
- **Invalidation**: Explicit cache invalidation required

## Important Files

- `appsettings.json` - Configuration
- `appsettings.Development.json` - Development environment config
- `appsettings.Production.json` - Production environment config
- `appsettings.Staging.json` - Staging environment config
- `GrcBlazorModule.cs` - Main Blazor module configuration
- `GrcApplicationModule.cs` - Application module with AutoMapper registration
- `GrcDbContext.cs` - Database context
- `Directory.Build.props` - Solution-wide build properties
- `.editorconfig` - Code style configuration

## Documentation References

For more details, see:
- `README.md` - Project overview
- `QUICK_REFERENCE.md` - Entities, services, pages, permissions
- `BUILD_INSTRUCTIONS.md` - Recent fixes and packages
- `COMPLETE_SYSTEM_INVENTORY.md` - Full coverage of all components
- `docs/DEVELOPMENT_SETUP.md` - Development environment setup

## Security Considerations

- All endpoints require authentication by default
- Use `[Authorize]` attribute with specific permission constants
- **Mandatory policy enforcement**: Call `EnforceAsync()` in all AppServices
- Role-based access control via ABP permissions
- Audit logging enabled for all policy decisions
- Multi-tenant safe implementation
- Policy-based data classification and ownership requirements

## Policy Rules (Baseline)

Default policy rules in `etc/policies/grc-baseline.yml`:

1. **REQUIRE_DATA_CLASSIFICATION**: All resources must have data classification
2. **REQUIRE_OWNER**: All resources must have an owner
3. **PROD_RESTRICTED_MUST_HAVE_APPROVAL**: Restricted data in prod requires approval
4. **NORMALIZE_EMPTY_LABELS**: Normalize invalid owner values

## When Making Changes

1. ✅ Follow existing code patterns and conventions
2. ✅ Inherit from `BasePolicyAppService` for all application services
3. ✅ **Always call `EnforceAsync()` before sensitive operations**
4. ✅ Add error handling with try/catch
5. ✅ Use ABP's localization for user-facing strings
6. ✅ Update AutoMapper profiles for new entities
7. ✅ Add permissions to `GrcPermissions.cs`
8. ✅ Update Arabic menus in `GrcMenuContributor.cs`
9. ✅ Follow StyleCop and .editorconfig rules
10. ✅ Update related unit tests
11. ✅ Check for breaking changes in APIs
12. ❌ Don't commit secrets or connection strings
13. ❌ Don't bypass authentication/authorization
14. ❌ Don't bypass policy enforcement
15. ❌ Don't use inline AutoMapper mapping in services
