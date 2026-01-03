# CLAUDE.md - Project Instructions for Claude Code

## Project Overview

This is the **GRC System** (Governance, Risk, and Compliance) - an enterprise application built with ABP Framework and Blazor Server.

## Technology Stack

- **Framework**: ABP Framework 8.3.0
- **UI**: Blazor Server with Basic Theme
- **Backend**: .NET 8.0
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: ABP Identity with Azure AD integration
- **Caching**: Redis
- **Deployment**: Docker, Kubernetes (Azure)

## Project Structure

```
src/
├── Grc.Blazor/              # Blazor Server UI application
├── Grc.Application/          # Application services layer
├── Grc.Application.Contracts/ # DTOs and interfaces
├── Grc.Domain/               # Domain entities and business logic
├── Grc.Domain.Shared/        # Shared constants, enums, localization
└── Grc.EntityFrameworkCore/  # EF Core DbContext and migrations
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

### Database
- PostgreSQL with EF Core
- Migrations in `Grc.EntityFrameworkCore/Migrations`
- Use `GrcDbContext` for data access

### Localization
- Resources in `Grc.Domain.Shared/Localization/Grc`
- Support for English (en) and Arabic (ar)

## Common Tasks

### Adding a New Entity
1. Create entity in `Grc.Domain/Entities`
2. Add DbSet to `GrcDbContext`
3. Create DTOs in `Grc.Application.Contracts`
4. Create AppService in `Grc.Application`
5. Add migration: `dotnet ef migrations add <Name>`

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

## Important Files

- `appsettings.json` - Configuration
- `GrcBlazorModule.cs` - Main module configuration
- `GrcDbContext.cs` - Database context
- `docker-compose.yml` - Local development environment

## Security Considerations

- All endpoints require authentication by default
- Use `[Authorize]` attribute for protected actions
- Role-based access control via ABP permissions
- Audit logging enabled for all entities

## When Making Changes

1. ✅ Follow existing code patterns
2. ✅ Add error handling with try/catch
3. ✅ Use ABP's localization for user-facing strings
4. ✅ Update related unit tests
5. ✅ Check for breaking changes in APIs
6. ❌ Don't commit secrets or connection strings
7. ❌ Don't bypass authentication/authorization
