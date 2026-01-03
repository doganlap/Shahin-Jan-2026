# GRC System - Developer Guide

## Overview

This guide helps developers get started with the GRC System, understand the codebase structure, and follow development best practices.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Project Structure](#project-structure)
3. [Development Workflow](#development-workflow)
4. [Coding Standards](#coding-standards)
5. [Common Tasks](#common-tasks)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

## Getting Started

### Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server 2019+** or **SQL Server Express** - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Visual Studio 2022** / **VS Code** / **Rider** - IDE of choice
- **Git** - Version control
- **Docker Desktop** (optional, for containerized development)

### Initial Setup

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd grc-system
   ```

2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

3. **Configure Database Connection**
   
   Edit `src/Grc.HttpApi.Host/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=localhost;Database=GrcDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run Database Migrations**
   ```bash
   cd src/Grc.DbMigrator
   dotnet run
   ```
   
   This will:
   - Create the database
   - Run all migrations
   - Seed default roles
   - Create admin user (username: `admin`, password: `1q2w3E*`)

5. **Start the API Host**
   ```bash
   cd src/Grc.HttpApi.Host
   dotnet run
   ```
   
   API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`
   - Swagger: `http://localhost:5000/swagger`

6. **Start the Blazor Application** (in a separate terminal)
   ```bash
   cd src/Grc.Blazor
   dotnet run
   ```
   
   Blazor will be available at:
   - `http://localhost:8080`

7. **Login**
   - Navigate to `http://localhost:8080`
   - Username: `admin`
   - Password: `1q2w3E*` (⚠️ Change in production!)

## Project Structure

```
grc-system/
├── src/
│   ├── Grc.Domain.Shared/              # Shared constants, permissions, localization
│   │   ├── Permissions/
│   │   │   └── GrcPermissions.cs       # Permission constants
│   │   └── Localization/
│   │       ├── GrcResource.ar.json     # Arabic translations
│   │       └── GrcResource.en.json    # English translations
│   │
│   ├── Grc.Application.Contracts/     # Application contracts
│   │   ├── Permissions/
│   │   │   └── GrcPermissionDefinitionProvider.cs
│   │   └── [Module]/                   # DTOs and interfaces per module
│   │       ├── Create[Entity]Dto.cs
│   │       ├── Update[Entity]Dto.cs
│   │       ├── [Entity]Dto.cs
│   │       └── I[Entity]AppService.cs
│   │
│   ├── Grc.Application/                 # Application layer
│   │   ├── Policy/                      # Policy engine
│   │   │   ├── PolicyContext.cs
│   │   │   ├── IPolicyEnforcer.cs
│   │   │   ├── PolicyEnforcer.cs
│   │   │   ├── PolicyStore.cs
│   │   │   ├── DotPathResolver.cs
│   │   │   ├── MutationApplier.cs
│   │   │   └── PolicyModels/
│   │   ├── Validators/                  # FluentValidation validators
│   │   │   └── [Entity]Validators.cs
│   │   ├── [Module]/                    # Application services
│   │   │   ├── [Entity]AppService.cs
│   │   │   └── [Entity]AutoMapperProfile.cs
│   │   └── GrcApplicationModule.cs
│   │
│   ├── Grc.Domain/                       # Domain layer
│   │   ├── [Module]/                     # Domain entities
│   │   │   └── [Entity].cs
│   │   ├── Seed/                         # Seed data contributors
│   │   │   ├── GrcRoleDataSeedContributor.cs
│   │   │   └── GrcAdminUserDataSeedContributor.cs
│   │   └── GrcDomainModule.cs
│   │
│   ├── Grc.EntityFrameworkCore/          # Data access layer
│   │   ├── GrcDbContext.cs
│   │   ├── EntityConfigurations/
│   │   └── GrcEntityFrameworkCoreModule.cs
│   │
│   ├── Grc.HttpApi.Host/                 # API host
│   │   ├── Program.cs                    # Application entry point
│   │   ├── Configuration/
│   │   │   └── ConfigurationValidator.cs
│   │   ├── Middleware/
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   ├── GlobalExceptionHandlerMiddleware.cs
│   │   │   ├── SecurityHeadersMiddleware.cs
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   └── RequestMetricsMiddleware.cs
│   │   ├── HealthChecks/
│   │   ├── Telemetry/
│   │   └── appsettings.*.json
│   │
│   ├── Grc.Blazor/                       # Blazor WebAssembly UI
│   │   ├── Pages/                        # Razor pages
│   │   │   ├── [Module]/
│   │   │   │   ├── Index.razor
│   │   │   │   ├── Create.razor
│   │   │   │   └── Edit.razor
│   │   ├── Services/
│   │   │   ├── ApiClientService.cs
│   │   │   ├── AuthenticationService.cs
│   │   │   └── ErrorToastService.cs
│   │   ├── Menus/
│   │   │   └── GrcMenuContributor.cs
│   │   └── GrcBlazorModule.cs
│   │
│   └── Grc.DbMigrator/                   # Database migrator
│       └── Program.cs
│
├── etc/
│   └── policies/                          # YAML policy files
│       └── grc-baseline.yml
│
├── docs/                                  # Documentation
│   ├── ARCHITECTURE.md
│   ├── DEPLOYMENT.md
│   ├── DEVELOPER_GUIDE.md
│   ├── CI_CD.md
│   ├── MONITORING.md
│   └── SECRETS_MANAGEMENT.md
│
├── .github/
│   └── workflows/                         # CI/CD pipelines
│
└── docker-compose.yml                     # Docker orchestration
```

## Development Workflow

### 1. Creating a New Entity

**Step 1: Create Domain Entity**
```csharp
// src/Grc.Domain/[Module]/[Entity].cs
namespace Grc.Domain.[Module];

public class [Entity] : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // IGovernedResource implementation
    public string ResourceType => "[Entity]";
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
}
```

**Step 2: Create DTOs**
```csharp
// src/Grc.Application.Contracts/[Module]/Create[Entity]Dto.cs
namespace Grc.Application.Contracts.[Module];

public class Create[Entity]Dto
{
    [Required]
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
}
```

**Step 3: Create Validator**
```csharp
// src/Grc.Application/Validators/[Entity]Validators.cs
public class Create[Entity]DtoValidator : AbstractValidator<Create[Entity]Dto>
{
    public Create[Entity]DtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}
```

**Step 4: Create AppService**
```csharp
// src/Grc.Application/[Module]/[Entity]AppService.cs
[Authorize(GrcPermissions.[Entity].View)]
public class [Entity]AppService : BasePolicyAppService, I[Entity]AppService
{
    private readonly IRepository<[Entity], Guid> _repository;

    public [Entity]AppService(
        IRepository<[Entity], Guid> repository,
        IPolicyEnforcer policyEnforcer)
        : base(policyEnforcer)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.[Entity].Create)]
    public async Task<[Entity]Dto> CreateAsync(Create[Entity]Dto input)
    {
        var entity = ObjectMapper.Map<Create[Entity]Dto, [Entity]>(input);
        
        await EnforceAsync("create", "[Entity]", entity);
        
        await _repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<[Entity], [Entity]Dto>(entity);
    }
}
```

**Step 5: Create AutoMapper Profile**
```csharp
// src/Grc.Application/[Module]/[Entity]AutoMapperProfile.cs
public class [Entity]AutoMapperProfile : Profile
{
    public [Entity]AutoMapperProfile()
    {
        CreateMap<Create[Entity]Dto, [Entity]>();
        CreateMap<Update[Entity]Dto, [Entity]>();
        CreateMap<[Entity], [Entity]Dto>();
    }
}
```

**Step 6: Configure Entity Framework**
```csharp
// src/Grc.EntityFrameworkCore/EntityConfigurations/[Entity]Configuration.cs
public class [Entity]Configuration : IEntityTypeConfiguration<[Entity]>
{
    public void Configure(EntityTypeBuilder<[Entity]> builder)
    {
        builder.ToTable("[Entities]");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
    }
}
```

**Step 7: Add to DbContext**
```csharp
// src/Grc.EntityFrameworkCore/GrcDbContext.cs
public DbSet<[Entity]> [Entities] { get; set; }
```

**Step 8: Create Migration**
```bash
cd src/Grc.EntityFrameworkCore
dotnet ef migrations add Add[Entity]Entity
```

### 2. Adding a New Permission

**Step 1: Add Permission Constant**
```csharp
// src/Grc.Domain.Shared/Permissions/GrcPermissions.cs
public static class [Module]
{
    public const string Default = GroupName + ".[Module]";
    public const string View = Default + ".View";
    public const string Create = Default + ".Create";
    public const string Update = Default + ".Update";
    public const string Delete = Default + ".Delete";
}
```

**Step 2: Register Permission**
```csharp
// src/Grc.Application.Contracts/Permissions/GrcPermissionDefinitionProvider.cs
var module = grc.AddPermission(GrcPermissions.[Module].Default, L("Permission:[Module]"));
module.AddChild(GrcPermissions.[Module].View, L("Permission:View"));
module.AddChild(GrcPermissions.[Module].Create, L("Permission:Create"));
// ...
```

**Step 3: Add to Menu**
```csharp
// src/Grc.Blazor/Menus/GrcMenuContributor.cs
m.AddItem(new ApplicationMenuItem("Grc.[Module]", "[Module Name]", "/[module]", icon: "fas fa-icon")
    .RequirePermissions(GrcPermissions.[Module].View));
```

### 3. Adding a New Policy Rule

**Step 1: Edit Policy YAML**
```yaml
# etc/policies/grc-baseline.yml
rules:
  - id: REQUIRE_[RULE_NAME]
    priority: 40
    description: "Description of the rule"
    enabled: true
    match:
      resource:
        type: "[Entity]"
    when:
      - op: equals
        path: "metadata.labels.[field]"
        value: "[value]"
    effect: deny
    severity: high
    message: "Error message"
    remediation:
      hint: "How to fix it"
```

**Step 2: Test the Rule**
```csharp
// In your AppService
await EnforceAsync("create", "[Entity]", entity);
```

## Coding Standards

### Naming Conventions

- **Classes**: PascalCase (e.g., `EvidenceAppService`)
- **Methods**: PascalCase (e.g., `CreateAsync`)
- **Properties**: PascalCase (e.g., `Name`, `CreatedAt`)
- **Private fields**: camelCase with underscore prefix (e.g., `_repository`)
- **Constants**: PascalCase (e.g., `GroupName`)
- **Interfaces**: PascalCase with `I` prefix (e.g., `IPolicyEnforcer`)

### Code Organization

1. **One class per file**
2. **Namespace matches folder structure**
3. **Group related classes in folders**
4. **Use regions for large files** (sparingly)

### Async/Await

- **Always use async/await** for I/O operations
- **Use `Task<T>`** for methods that return values
- **Use `Task`** for methods that don't return values
- **Never use `.Result` or `.Wait()`** - use `await` instead

### Error Handling

- **Use `BusinessException`** for business logic errors
- **Use `PolicyViolationException`** for policy violations
- **Let middleware handle exceptions** - don't catch and swallow
- **Log errors** with appropriate log levels

### Validation

- **Use FluentValidation** for DTO validation
- **Add validation rules** in validators, not in AppServices
- **Return Arabic error messages** for user-facing errors

### Documentation

- **Add XML comments** for public APIs
- **Document complex logic** with inline comments
- **Keep README files updated**

## Common Tasks

### Running Migrations

```bash
cd src/Grc.DbMigrator
dotnet run
```

### Adding a New Migration

```bash
cd src/Grc.EntityFrameworkCore
dotnet ef migrations add MigrationName
```

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Cleaning Build Artifacts

```bash
dotnet clean
```

### Updating NuGet Packages

```bash
dotnet restore
dotnet list package --outdated
```

## Testing

### Unit Tests

Create test projects:
- `Grc.Application.Tests`
- `Grc.Domain.Tests`
- `Grc.EntityFrameworkCore.Tests`

### Example Unit Test

```csharp
[Fact]
public async Task CreateAsync_Should_Enforce_Policy()
{
    // Arrange
    var dto = new CreateEvidenceDto { Name = "Test" };
    
    // Act & Assert
    await Assert.ThrowsAsync<PolicyViolationException>(
        () => _appService.CreateAsync(dto));
}
```

### Integration Tests

Test full request/response cycle:
```csharp
[Fact]
public async Task POST_Evidence_Should_Return_201()
{
    var response = await _client.PostAsync("/api/evidence", content);
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

## Troubleshooting

### Build Errors

**Error**: `CS0246: The type or namespace name 'X' could not be found`
- **Solution**: Check namespace imports
- **Solution**: Verify project references

**Error**: `CS1705: Assembly 'X' uses version 'Y' which has a higher version`
- **Solution**: Update package versions in `Directory.Build.props`
- **Solution**: Use explicit `PackageReference` in `.csproj`

### Runtime Errors

**Error**: `Configuration validation failed`
- **Solution**: Check `appsettings.json` for missing values
- **Solution**: Set environment variables

**Error**: `Database connection failed`
- **Solution**: Verify connection string
- **Solution**: Check SQL Server is running
- **Solution**: Verify firewall rules

**Error**: `Policy file not found`
- **Solution**: Verify `etc/policies/grc-baseline.yml` exists
- **Solution**: Check file path in configuration

### Debugging Tips

1. **Enable Detailed Logging**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug"
       }
     }
   }
   ```

2. **Use Swagger UI**
   - Navigate to `/swagger`
   - Test API endpoints directly
   - View request/response schemas

3. **Check Application Insights**
   - View telemetry in Azure Portal
   - Check for exceptions and performance issues

4. **Review Logs**
   - Console output (development)
   - File logs: `logs/grc-YYYYMMDD.txt`
   - Application Insights (production)

## Resources

- [ABP Framework Documentation](https://docs.abp.io/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

## Getting Help

- **Documentation**: Check `/docs` folder
- **Issues**: Create GitHub issue
- **Questions**: Contact development team

---

**Last Updated:** 2026-01-02
