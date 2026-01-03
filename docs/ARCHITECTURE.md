# GRC System - Architecture Documentation

## Overview

The GRC (Governance, Risk, Compliance) System is built on ABP Framework 8.3.0 with .NET 8.0, using Blazor WebAssembly for the frontend and ASP.NET Core for the backend API.

## Visual Architecture Diagrams

### 1. System Layered Architecture

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

### 2. Policy Enforcement Flow

```mermaid
sequenceDiagram
    participant User
    participant Blazor
    participant AppService
    participant PolicyEnforcer
    participant PolicyStore
    participant DotPathResolver
    participant MutationApplier
    participant AuditLogger
    participant Database
    
    User->>Blazor: Create Evidence Action
    Blazor->>AppService: POST /api/evidence
    AppService->>AppService: Check Permission<br/>Grc.Evidence.Upload
    AppService->>PolicyEnforcer: EnforceAsync(context)
    
    PolicyEnforcer->>PolicyStore: LoadPolicyAsync()
    PolicyStore-->>PolicyEnforcer: Policy Document (YAML)
    
    PolicyEnforcer->>PolicyEnforcer: Get Active Exceptions
    PolicyEnforcer->>PolicyEnforcer: Sort Rules by Priority
    
    loop For Each Rule
        PolicyEnforcer->>PolicyEnforcer: Check Exception Match
        alt Exception Matches
            PolicyEnforcer->>PolicyEnforcer: Skip Rule
        else No Exception
            PolicyEnforcer->>PolicyEnforcer: Match Rule<br/>(Resource Type, Environment)
            alt Rule Matches
                PolicyEnforcer->>DotPathResolver: Evaluate Conditions<br/>(when clauses)
                DotPathResolver-->>PolicyEnforcer: Condition Result
                alt Condition True
                    PolicyEnforcer->>PolicyEnforcer: Apply Rule Effect
                    alt Effect = deny
                        PolicyEnforcer->>AuditLogger: Log Decision
                        PolicyEnforcer-->>AppService: PolicyViolationException
                        AppService-->>Blazor: 400 Bad Request<br/>with remediation hint
                    else Effect = mutate
                        PolicyEnforcer->>MutationApplier: Apply Mutations
                        MutationApplier-->>PolicyEnforcer: Mutated Resource
                    else Effect = allow
                        PolicyEnforcer->>PolicyEnforcer: Continue
                    end
                end
            end
        end
    end
    
    alt All Rules Passed
        PolicyEnforcer->>AuditLogger: Log Decision (allow)
        PolicyEnforcer-->>AppService: Success
        AppService->>Database: Insert Entity
        Database-->>AppService: Entity Saved
        AppService-->>Blazor: EvidenceDto
        Blazor-->>User: Success Toast
    end
```

### 3. Authorization and Permission Flow

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

### 4. Request Processing Flow (End-to-End)

```mermaid
sequenceDiagram
    participant Browser
    participant Blazor
    participant ApiClient
    participant Middleware
    participant Auth
    participant AppService
    participant Policy
    participant Repository
    participant Database
    
    Browser->>Blazor: User Interaction
    Blazor->>Blazor: Validate Permission<br/>(UI Level)
    Blazor->>ApiClient: PostAsync(endpoint, dto)
    
    ApiClient->>Middleware: HTTP Request<br/>with JWT Token
    
    Middleware->>Middleware: CorrelationIdMiddleware<br/>Add Correlation ID
    Middleware->>Middleware: SecurityHeadersMiddleware<br/>Add Security Headers
    Middleware->>Middleware: GlobalExceptionHandler<br/>Wrap in try-catch
    
    Middleware->>Auth: Validate JWT Token
    Auth-->>Middleware: User Identity + Roles
    
    Middleware->>Auth: Check Permission<br/>[Authorize] Attribute
    Auth-->>Middleware: Permission Granted
    
    Middleware->>AppService: Route to AppService<br/>e.g., EvidenceAppService.CreateAsync()
    
    AppService->>AppService: FluentValidation<br/>Validate DTO
    
    AppService->>Policy: EnforceAsync(action, resourceType, resource)
    Policy->>Policy: Load Policy from YAML
    Policy->>Policy: Evaluate Rules
    Policy-->>AppService: Allow/Deny Decision
    
    alt Policy Denies
        AppService-->>Middleware: PolicyViolationException
        Middleware-->>ApiClient: 400 Bad Request<br/>with remediation
        ApiClient-->>Blazor: Error Response
        Blazor-->>Browser: Error Toast
    else Policy Allows
        AppService->>AppService: Map DTO to Entity<br/>(AutoMapper)
        AppService->>Repository: InsertAsync(entity)
        Repository->>Database: INSERT INTO Evidence
        Database-->>Repository: Entity Saved
        Repository-->>AppService: Entity with ID
        AppService->>AppService: Map Entity to DTO
        AppService-->>Middleware: EvidenceDto
        Middleware-->>ApiClient: 200 OK with DTO
        ApiClient-->>Blazor: Success Response
        Blazor-->>Browser: Success Toast + Update UI
    end
```

### 5. Component Interaction Diagram

```mermaid
graph LR
    subgraph ui["UI Components"]
        Menu[GrcMenuContributor]
        Pages[Blazor Pages]
        Client[ApiClientService]
    end
    
    subgraph services["Application Services"]
        EvidenceSvc[EvidenceAppService]
        RiskSvc[RiskAppService]
        AssessmentSvc[AssessmentAppService]
        BaseSvc[BasePolicyAppService]
    end
    
    subgraph policy["Policy Engine"]
        Enforcer[PolicyEnforcer]
        Store[PolicyStore]
        Resolver[DotPathResolver]
        Mutator[MutationApplier]
        Logger[PolicyAuditLogger]
    end
    
    subgraph permissions["Permission System"]
        PermDefs[GrcPermissions]
        PermProvider[GrcPermissionDefinitionProvider]
        RoleResolver[RoleResolver]
    end
    
    subgraph domain["Domain"]
        Entities[Domain Entities]
        Repos[Repositories]
    end
    
    subgraph data["Data Access"]
        EF[EF Core]
        DB[(SQL Server)]
    end
    
    Menu --> PermDefs
    Pages --> Client
    Client --> EvidenceSvc
    EvidenceSvc --> BaseSvc
    RiskSvc --> BaseSvc
    AssessmentSvc --> BaseSvc
    BaseSvc --> Enforcer
    BaseSvc --> RoleResolver
    Enforcer --> Store
    Enforcer --> Resolver
    Enforcer --> Mutator
    Enforcer --> Logger
    Enforcer --> PermProvider
    BaseSvc --> Entities
    Entities --> Repos
    Repos --> EF
    EF --> DB
    PermProvider --> PermDefs
```

### 6. Policy Rule Evaluation Logic

```mermaid
flowchart TD
    Start([PolicyEnforcer.EnforceAsync]) --> LoadPolicy[Load Policy from YAML]
    LoadPolicy --> GetExceptions[Get Active Exceptions<br/>Not Expired + Match Context]
    GetExceptions --> SortRules[Sort Rules by Priority<br/>Ascending Order]
    
    SortRules --> LoopStart{For Each Rule}
    
    LoopStart --> CheckException{Exception<br/>Applies?}
    CheckException -->|Yes| SkipRule[Skip Rule]
    CheckException -->|No| MatchRule{Match Rule?<br/>Resource Type<br/>Environment<br/>Principal}
    
    MatchRule -->|No| NextRule[Next Rule]
    MatchRule -->|Yes| EvalConditions[Evaluate When Conditions<br/>Dot-Path Resolution]
    
    EvalConditions --> AllTrue{All Conditions<br/>True?}
    AllTrue -->|No| NextRule
    AllTrue -->|Yes| CheckEffect{Effect Type?}
    
    CheckEffect -->|deny| DenyDecision[Decision: DENY]
    CheckEffect -->|allow| AllowDecision[Decision: ALLOW]
    CheckEffect -->|mutate| ApplyMutation[Apply Mutations<br/>Update Resource]
    CheckEffect -->|audit| LogAudit[Log Audit Event]
    
    ApplyMutation --> ContinueEval[Continue Evaluation]
    LogAudit --> ContinueEval
    AllowDecision --> CheckShortCircuit{Short Circuit<br/>Enabled?}
    
    CheckShortCircuit -->|Yes| StopEval[Stop Evaluation]
    CheckShortCircuit -->|No| NextRule
    
    DenyDecision --> ConflictStrategy{Conflict Strategy?}
    ConflictStrategy -->|denyOverrides| FinalDeny[Final: DENY]
    ConflictStrategy -->|allowOverrides| CheckOther[Check Other Rules]
    ConflictStrategy -->|highestPriority| UsePriority[Use Highest Priority]
    
    SkipRule --> NextRule
    NextRule --> LoopStart
    ContinueEval --> NextRule
    
    StopEval --> FinalAllow[Final: ALLOW]
    FinalDeny --> ThrowException[Throw PolicyViolationException]
    FinalAllow --> LogDecision[Log Decision to Audit]
    ThrowException --> End([End])
    LogDecision --> End
```

### 7. Data Flow: Evidence Creation Example

```mermaid
flowchart LR
    subgraph input["Input"]
        UserInput[User Fills Form<br/>CreateEvidenceDto]
    end
    
    subgraph validation["Validation"]
        FluentVal[FluentValidation<br/>Validate DTO]
        PolicyVal[Policy Enforcement<br/>Check Rules]
    end
    
    subgraph transformation["Transformation"]
        AutoMap[AutoMapper<br/>DTO → Entity]
        Mutation[Apply Mutations<br/>if needed]
    end
    
    subgraph persistence["Persistence"]
        Repo[Repository<br/>InsertAsync]
        EF[EF Core<br/>SaveChanges]
        DB[(Database)]
    end
    
    subgraph output["Output"]
        EntityMap[AutoMapper<br/>Entity → DTO]
        Response[Return EvidenceDto]
    end
    
    UserInput --> FluentVal
    FluentVal --> PolicyVal
    PolicyVal --> AutoMap
    AutoMap --> Mutation
    Mutation --> Repo
    Repo --> EF
    EF --> DB
    DB --> EntityMap
    EntityMap --> Response
```

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Blazor WebAssembly (Grc.Blazor)               │  │
│  │  - Razor Pages (56 pages)                              │  │
│  │  - ApiClientService (HTTP communication)               │  │
│  │  - ErrorToastService (User notifications)              │  │
│  │  - Arabic Menu (GrcMenuContributor)                    │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP/REST
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      API Layer                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      HTTP API Host (Grc.HttpApi.Host)                 │  │
│  │  - Swagger/OpenAPI                                    │  │
│  │  - JWT Authentication                                 │  │
│  │  - CORS Configuration                                 │  │
│  │  - Health Checks (/health, /health/ready, /health/live)│  │
│  │  - Global Exception Handler                           │  │
│  │  - Correlation ID Middleware                          │  │
│  │  - Security Headers Middleware                         │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Dependency Injection
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      Application Services (Grc.Application)          │  │
│  │  - 19 AppServices (CRUD operations)                   │  │
│  │  - Policy Engine (PolicyEnforcer)                     │  │
│  │  - Policy Store (YAML loader)                        │  │
│  │  - FluentValidation validators                        │  │
│  │  - AutoMapper profiles                                │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      Application Contracts (Grc.Application.Contracts)│ │
│  │  - DTOs (CreateDto, UpdateDto, ListDto)               │  │
│  │  - AppService interfaces                              │  │
│  │  - Permission definitions                             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Repository Pattern
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      Domain Entities (Grc.Domain)                     │  │
│  │  - 14 Domain Entities                                 │  │
│  │  - IGovernedResource interface                        │  │
│  │  - Repository interfaces                              │  │
│  │  - Seed data contributors                             │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      Domain Shared (Grc.Domain.Shared)                │  │
│  │  - Permissions (GrcPermissions)                       │  │
│  │  - Role definitions                                   │  │
│  │  - Localization                                       │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Entity Framework Core
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      EntityFrameworkCore (Grc.EntityFrameworkCore)    │  │
│  │  - GrcDbContext                                       │  │
│  │  - Entity configurations                              │  │
│  │  - Migrations                                         │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      Database (SQL Server)                            │  │
│  │  - GrcDb database                                     │  │
│  │  - Identity tables (ABP)                              │  │
│  │  - GRC entity tables                                  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Component Details

### 1. Blazor WebAssembly Frontend

**Project:** `Grc.Blazor`

**Technologies:**
- Blazor WebAssembly 8.0
- ABP Framework Blazor components
- Bootstrap (via ABP theme)

**Key Components:**
- **ApiClientService:** HTTP client wrapper with error handling
- **ErrorToastService:** Toast notifications for errors/success
- **GrcMenuContributor:** Arabic menu with permission-based visibility
- **56 Razor Pages:** Complete UI for all GRC modules

**Communication:**
- Calls backend API via `ApiClientService`
- Base URL: `/api/`
- Handles JWT authentication
- Displays policy violations with remediation hints

### 2. HTTP API Host

**Project:** `Grc.HttpApi.Host`

**Technologies:**
- ASP.NET Core 8.0
- ABP Framework 8.3.0
- Swagger/OpenAPI
- Serilog for logging
- Application Insights (optional)

**Middleware Pipeline:**
1. CorrelationIdMiddleware - Request tracking
2. SecurityHeadersMiddleware - Security headers
3. GlobalExceptionHandlerMiddleware - Error handling
4. CORS
5. Authentication (JWT)
6. Authorization
7. Health Checks
8. Controllers

**Endpoints:**
- `/health` - Overall health
- `/health/ready` - Readiness (database, policy store)
- `/health/live` - Liveness (self check)
- `/swagger` - API documentation (Development)
- `/api/*` - Application endpoints

### 3. Application Layer

**Project:** `Grc.Application`

**Responsibilities:**
- Business logic
- Policy enforcement
- Data transformation (AutoMapper)
- Input validation (FluentValidation)

**Key Services:**
- **19 AppServices:** One per entity (Evidence, Risk, Assessment, etc.)
- **PolicyEnforcer:** Deterministic policy evaluation
- **PolicyStore:** YAML policy loading and caching
- **DotPathResolver:** Dot-path value resolution
- **MutationApplier:** Resource mutations

**Policy Integration:**
All AppServices inherit from `BasePolicyAppService` which provides:
- `EnforceAsync()` method
- Automatic policy evaluation
- Policy violation exceptions

### 4. Domain Layer

**Projects:** `Grc.Domain`, `Grc.Domain.Shared`

**Entities:**
- Evidence
- Assessment
- Risk
- Audit
- ActionPlan
- PolicyDocument
- RegulatoryFramework
- Regulator
- Vendor
- ComplianceEvent
- ControlAssessment
- Workflow
- Notification
- Subscription

**Interfaces:**
- `IGovernedResource`: Metadata interface for policy evaluation

**Seed Data:**
- `GrcRoleDataSeedContributor`: Creates 11 predefined roles
- `GrcAdminUserDataSeedContributor`: Creates admin user

### 5. Policy Engine

**Location:** `Grc.Application/Policy/`

**Components:**

1. **PolicyContext:** Evaluation context
   - Action (create/update/submit/approve)
   - Environment (dev/staging/prod)
   - ResourceType
   - Resource (entity/DTO)
   - Principal (user/roles)

2. **PolicyEnforcer:** Main evaluation engine
   - Loads policy from YAML
   - Evaluates rules by priority
   - Handles exceptions
   - Applies mutations
   - Returns decision

3. **PolicyStore:** Policy loading
   - Loads from `etc/policies/`
   - Caches policy in memory
   - Supports hot reload

4. **DotPathResolver:** Value resolution
   - Resolves dot-paths (e.g., `metadata.labels.dataClassification`)
   - Supports nested objects
   - Handles null values

5. **MutationApplier:** Resource mutations
   - Applies mutations from rules
   - Supports set/remove/add operations
   - Deterministic application

**Policy File:** `etc/policies/grc-baseline.yml`

**Rules:**
- REQUIRE_DATA_CLASSIFICATION
- REQUIRE_OWNER
- PROD_RESTRICTED_MUST_HAVE_APPROVAL
- NORMALIZE_EMPTY_LABELS

### 6. Database

**Technology:** SQL Server 2022

**Tables:**
- ABP Identity tables (Users, Roles, Permissions)
- ABP Tenant Management tables
- GRC entity tables (14 entities)
- Audit log tables (ABP)

**Migrations:**
- Managed via Entity Framework Core
- Run via `Grc.DbMigrator` project
- Initial migration: `20260102091922_Initial.cs`

## Data Flow

### Create Evidence Flow

```
1. User fills form in Blazor
   ↓
2. Blazor calls ApiClientService.PostAsync("/api/evidence", dto)
   ↓
3. API Host receives request
   ↓
4. GlobalExceptionHandlerMiddleware catches errors
   ↓
5. CorrelationIdMiddleware adds correlation ID
   ↓
6. SecurityHeadersMiddleware adds security headers
   ↓
7. JWT Authentication validates token
   ↓
8. Authorization checks GrcPermissions.Evidence.Upload
   ↓
9. EvidenceAppService.CreateAsync() called
   ↓
10. FluentValidation validates DTO
   ↓
11. PolicyEnforcer.EnforceAsync() evaluates policies
    - Checks data classification
    - Checks owner
    - Applies mutations if needed
   ↓
12. Entity mapped from DTO (AutoMapper)
   ↓
13. Repository.InsertAsync() saves to database
   ↓
14. DTO returned to client
   ↓
15. Blazor displays success toast
```

### Policy Evaluation Flow

```
1. PolicyEnforcer.EnforceAsync(context)
   ↓
2. PolicyStore.LoadPolicyAsync() - loads YAML
   ↓
3. Check exceptions (match context + not expired)
   ↓
4. Sort rules by priority (ascending)
   ↓
5. For each rule:
   a. Check match conditions (resource type, environment)
   b. Evaluate "when" conditions (dot-path checks)
   c. If match:
      - If effect = "deny" → throw PolicyViolationException
      - If effect = "mutate" → apply mutations
      - If effect = "allow" → continue
      - If effect = "audit" → log and continue
   ↓
6. If no deny → allow
   ↓
7. Log decision (PolicyAuditLogger)
```

## Security Architecture

### Authentication
- **Method:** JWT Bearer tokens
- **Issuer:** GrcSystem
- **Audience:** GrcSystem
- **Expiration:** 3600 seconds (configurable)

### Authorization
- **Method:** ABP Permission system
- **Granularity:** Per-module, per-action
- **Examples:**
  - `Grc.Evidence.View` - View evidence
  - `Grc.Evidence.Upload` - Upload evidence
  - `Grc.Evidence.Approve` - Approve evidence

### Policy Enforcement
- **Location:** Backend only (no UI-only checks)
- **Timing:** Before database operations
- **Deterministic:** Same input = same decision

### Security Headers
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- X-XSS-Protection: 1; mode=block
- Content-Security-Policy: (configured)
- Referrer-Policy: strict-origin-when-cross-origin

## Multi-Tenancy

**Enabled:** Yes

**Mode:** Hybrid (shared database, separate schemas)

**Tenant Resolution:**
- From JWT token
- From subdomain
- From header

**Data Isolation:**
- All entities include TenantId
- Queries automatically filtered by tenant
- Cross-tenant access prevented

## Error Handling

### Global Exception Handler

**Location:** `GlobalExceptionHandlerMiddleware`

**Handles:**
- `BusinessException` → 400 Bad Request
- `PolicyViolationException` → 400 with remediation hint
- `UnauthorizedAccessException` → 401 Unauthorized
- `KeyNotFoundException` → 404 Not Found
- Generic exceptions → 500 Internal Server Error

**Response Format:**
```json
{
  "code": "Grc:PolicyViolation",
  "message": "Missing/invalid metadata.labels.dataClassification",
  "correlationId": "guid",
  "remediationHint": "Set metadata.labels.dataClassification to one of the allowed values.",
  "validationErrors": ["error1", "error2"]
}
```

## Logging

### Serilog Configuration

**Sinks:**
- Console (development)
- File (logs/grc-YYYYMMDD.txt)
- Application Insights (production, if configured)

**Enrichment:**
- Machine name
- Thread ID
- Correlation ID
- User ID (from ABP)

**Log Levels:**
- Development: Debug
- Production: Warning

## Monitoring

### Health Checks

**Endpoints:**
- `/health` - Overall health
- `/health/ready` - Readiness (database, policy store)
- `/health/live` - Liveness (self)

**Checks:**
- Database connectivity
- Policy store accessibility
- Self check

### Application Insights

**Features:**
- Request telemetry
- Dependency tracking
- Exception tracking
- Performance counters
- Custom metrics

## Deployment Architecture

### Development
- Local SQL Server
- API Host: localhost:5000
- Blazor: localhost:8080
- No containerization

### Staging/Production
- Docker containers
- Docker Compose orchestration
- SQL Server in container
- API Host in container
- Blazor served via nginx

## Scalability Considerations

### Horizontal Scaling
- Stateless API (can scale horizontally)
- Database connection pooling
- Caching (future: Redis)

### Performance
- Async/await throughout
- Database indexes on foreign keys
- Policy caching in memory
- Connection pooling

## Technology Stack

### Backend
- .NET 8.0
- ABP Framework 8.3.0
- Entity Framework Core 8.0
- SQL Server 2022
- Serilog
- FluentValidation
- AutoMapper
- YamlDotNet

### Frontend
- Blazor WebAssembly 8.0
- ABP Blazor components
- Bootstrap (via ABP theme)

### Infrastructure
- Docker
- Docker Compose
- GitHub Actions (CI/CD)
- Application Insights (monitoring)

## Design Patterns

1. **Repository Pattern:** Data access abstraction
2. **Unit of Work:** Transaction management (ABP)
3. **Dependency Injection:** Service registration (ABP Autofac)
4. **Policy Pattern:** Policy engine for business rules
5. **Strategy Pattern:** Conflict resolution strategies
6. **Middleware Pattern:** Request pipeline processing

## Future Enhancements

1. **Caching:** Redis for distributed caching
2. **Message Queue:** RabbitMQ/Azure Service Bus for async processing
3. **Search:** Elasticsearch for full-text search
4. **File Storage:** Azure Blob Storage / AWS S3
5. **Real-time:** SignalR for real-time updates

---

**Last Updated:** 2026-01-02
