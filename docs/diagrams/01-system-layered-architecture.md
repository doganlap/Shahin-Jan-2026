# System Layered Architecture

## Description
Shows the complete layered architecture from Blazor UI through API layer, Application layer, Domain layer, to Infrastructure layer following ABP Framework patterns.

## Diagram

```mermaid
graph TB
    subgraph presentation["Presentation Layer"]
        Blazor["Blazor WebAssembly<br/>Grc.Blazor"]
        Menu["Arabic Menu<br/>GrcMenuContributor"]
        Pages["56 Razor Pages"]
        ApiClient["ApiClientService"]
    end
    
    subgraph api["API Layer"]
        HttpApi["HTTP API Host<br/>Grc.HttpApi.Host"]
        Middleware["Middleware Pipeline<br/>Auth, CORS, Security"]
        Swagger["Swagger/OpenAPI"]
    end
    
    subgraph application["Application Layer"]
        AppServices["19 AppServices<br/>Evidence, Risk, Assessment..."]
        PolicyEngine["Policy Engine<br/>PolicyEnforcer"]
        PolicyStore["Policy Store<br/>YAML Loader"]
        Validators["FluentValidation"]
        Mapper["AutoMapper"]
    end
    
    subgraph contracts["Application Contracts"]
        DTOs["DTOs<br/>CreateDto, UpdateDto"]
        Interfaces["AppService Interfaces"]
        Permissions["Permission Definitions"]
    end
    
    subgraph domain["Domain Layer"]
        Entities["14 Domain Entities<br/>Evidence, Risk, Audit..."]
        Repositories["Repository Interfaces"]
        Seed["Seed Data Contributors"]
    end
    
    subgraph infrastructure["Infrastructure Layer"]
        EF["Entity Framework Core<br/>Grc.EntityFrameworkCore"]
        DbContext["GrcDbContext"]
        Database["SQL Server Database"]
    end
    
    Blazor --> ApiClient
    Menu --> Blazor
    Pages --> Blazor
    ApiClient -->|HTTP/REST| HttpApi
    HttpApi --> Middleware
    HttpApi --> Swagger
    Middleware --> AppServices
    AppServices --> PolicyEngine
    AppServices --> Validators
    AppServices --> Mapper
    PolicyEngine --> PolicyStore
    AppServices --> DTOs
    AppServices --> Interfaces
    AppServices --> Entities
    Entities --> Repositories
    Repositories --> EF
    EF --> DbContext
    DbContext --> Database
    Seed --> Entities
```

## Key Components

### Presentation Layer
- **Grc.Blazor**: Blazor WebAssembly application
- **GrcMenuContributor**: Arabic menu with permission-based visibility
- **56 Razor Pages**: Complete UI for all GRC modules
- **ApiClientService**: HTTP client wrapper for API communication

### API Layer
- **Grc.HttpApi.Host**: ASP.NET Core API host
- **Middleware Pipeline**: Authentication, CORS, Security Headers, Exception Handling
- **Swagger/OpenAPI**: API documentation

### Application Layer
- **19 AppServices**: Business logic for each entity
- **Policy Engine**: Deterministic policy evaluation
- **Policy Store**: YAML policy loader with caching
- **FluentValidation**: Input validation
- **AutoMapper**: DTO ↔ Entity mapping

### Domain Layer
- **14 Domain Entities**: Core business entities
- **Repository Interfaces**: Data access abstraction
- **Seed Data Contributors**: Initial data setup

### Infrastructure Layer
- **Entity Framework Core**: ORM
- **GrcDbContext**: Database context
- **SQL Server**: Database

## Related Files
- `src/Grc.Blazor/GrcBlazorModule.cs`
- `src/Grc.HttpApi.Host/GrcHttpApiHostModule.cs`
- `src/Grc.Application/GrcApplicationModule.cs`
- `src/Grc.Domain/GrcDomainModule.cs`
- `src/Grc.EntityFrameworkCore/GrcEntityFrameworkCoreModule.cs`
