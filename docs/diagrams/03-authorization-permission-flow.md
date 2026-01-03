# Authorization and Permission Flow

## Description
Complete authorization flow from user action through menu visibility, page access, API permission checks, and policy enforcement. Shows the two-tier security model (Permissions + Policies).

## Diagram

```mermaid
flowchart TD
    Start([User Action]) --> Auth{Authenticated?}
    Auth -->|No| Login[Redirect to Login]
    Auth -->|Yes| GetRoles[Get User Roles]
    
    GetRoles --> CheckMenu{Menu Item<br/>Permission Check}
    CheckMenu -->|No Permission| HideMenu[Hide Menu Item]
    CheckMenu -->|Has Permission| ShowMenu[Show Menu Item]
    
    ShowMenu --> UserClick[User Clicks Menu]
    UserClick --> PageLoad[Load Blazor Page]
    PageLoad --> AuthorizeView{AuthorizeView<br/>Component}
    
    AuthorizeView -->|No Permission| HideContent[Hide Protected Content]
    AuthorizeView -->|Has Permission| ShowContent[Show Page Content]
    
    ShowContent --> UserAction[User Performs Action<br/>e.g., Create Evidence]
    UserAction --> ApiCall[API Call to Backend]
    
    ApiCall --> JWT[Validate JWT Token]
    JWT -->|Invalid| Reject[401 Unauthorized]
    JWT -->|Valid| CheckPermission[Check Permission<br/>Authorize Attribute]
    
    CheckPermission -->|No Permission| Reject403[403 Forbidden]
    CheckPermission -->|Has Permission| ExecuteService[Execute AppService]
    
    ExecuteService --> PolicyCheck[Policy Enforcement]
    PolicyCheck -->|Deny| PolicyViolation[PolicyViolationException<br/>400 with remediation]
    PolicyCheck -->|Allow| Success[Operation Success]
    
    HideMenu --> End([End])
    HideContent --> End
    Reject --> End
    Reject403 --> End
    PolicyViolation --> End
    Success --> End
```

## Security Layers

### Layer 1: Authentication
- **JWT Token Validation**: Token validated on every API request
- **OpenIddict**: ABP Framework authentication provider
- **Result**: User identity and roles extracted

### Layer 2: Menu Permission Check
- **Location**: `GrcMenuContributor.cs`
- **Check**: `RequiredPermissionName` attribute
- **Result**: Menu items hidden if user lacks permission
- **Example**: `GrcPermissions.Evidence.View`

### Layer 3: Page Component Authorization
- **Location**: Blazor Razor pages
- **Check**: `AuthorizeView` component
- **Result**: Protected content hidden if no permission
- **Example**: Create button only visible with `Grc.Evidence.Upload`

### Layer 4: API Permission Check
- **Location**: AppService methods
- **Check**: `[Authorize(PermissionName)]` attribute
- **Result**: 403 Forbidden if no permission
- **Example**: `[Authorize(GrcPermissions.Evidence.Upload)]`

### Layer 5: Policy Enforcement
- **Location**: `BasePolicyAppService.EnforceAsync()`
- **Check**: Business rules from YAML policy
- **Result**: PolicyViolationException with remediation hint
- **Example**: Data classification required, owner required

## Related Files
- `src/Grc.Blazor/Menus/GrcMenuContributor.cs`
- `src/Grc.Domain.Shared/Permissions/GrcPermissions.cs`
- `src/Grc.Application.Contracts/Permissions/GrcPermissionDefinitionProvider.cs`
- `src/Grc.Application/Policy/BasePolicyAppService.cs`
- `src/Grc.Application/Evidence/EvidenceAppService.cs`
