# GRC System - Architecture Diagrams Summary

## Overview

This document provides a quick reference to all architecture diagrams in the GRC system documentation.

## Available Diagrams

### 1. System Layered Architecture
**Location:** `docs/ARCHITECTURE.md` - Section "Visual Architecture Diagrams"

**Description:** Shows the complete layered architecture from Blazor UI through API layer, Application layer, Domain layer, to Infrastructure layer.

**Key Components:**
- Presentation Layer: Blazor WebAssembly, Arabic Menu, 56 Razor Pages
- API Layer: HTTP API Host, Middleware Pipeline
- Application Layer: 19 AppServices, Policy Engine, Validators
- Domain Layer: 14 Domain Entities, Repositories
- Infrastructure Layer: EF Core, SQL Server Database

### 2. Policy Enforcement Flow
**Type:** Sequence Diagram

**Description:** Detailed sequence showing how policy evaluation works from user action through policy enforcement to final decision.

**Key Steps:**
1. User action triggers AppService
2. PolicyEnforcer loads policy from YAML
3. Rules evaluated by priority
4. Conditions checked via DotPathResolver
5. Effects applied (deny/allow/mutate/audit)
6. Decision logged and returned

### 3. Authorization and Permission Flow
**Type:** Flowchart

**Description:** Complete authorization flow from user action through menu visibility, page access, API permission checks, and policy enforcement.

**Key Decision Points:**
- Authentication check
- Menu permission check
- Page component authorization
- API permission check
- Policy enforcement

### 4. Request Processing Flow (End-to-End)
**Type:** Sequence Diagram

**Description:** Complete request lifecycle from browser interaction through all middleware layers, authentication, authorization, business logic, and database persistence.

**Key Stages:**
1. Browser → Blazor UI
2. Blazor → API Client
3. Middleware Pipeline (Correlation ID, Security Headers, Exception Handling)
4. Authentication (JWT validation)
5. Authorization (Permission check)
6. AppService execution
7. Policy enforcement
8. Database persistence
9. Response back to browser

### 5. Component Interaction Diagram
**Type:** Graph Diagram

**Description:** Shows relationships and dependencies between major system components.

**Component Groups:**
- UI Components (Menu, Pages, Client)
- Application Services (Evidence, Risk, Assessment, Base)
- Policy Engine (Enforcer, Store, Resolver, Mutator, Logger)
- Permission System (Permissions, Provider, RoleResolver)
- Domain (Entities, Repositories)
- Data Access (EF Core, Database)

### 6. Policy Rule Evaluation Logic
**Type:** Flowchart

**Description:** Detailed flowchart showing the deterministic policy evaluation algorithm.

**Key Logic:**
1. Load policy and get exceptions
2. Sort rules by priority
3. For each rule: check exceptions, match conditions, evaluate when clauses
4. Apply effects (deny/allow/mutate/audit)
5. Handle conflict strategies
6. Log decision

### 7. Data Flow: Evidence Creation Example
**Type:** Flowchart

**Description:** Step-by-step data flow for creating evidence, showing validation, transformation, persistence, and output stages.

**Stages:**
- Input: User form data
- Validation: FluentValidation + Policy
- Transformation: AutoMapper + Mutations
- Persistence: Repository → EF Core → Database
- Output: Entity → DTO → Response

## How to View Diagrams

All diagrams are written in Mermaid syntax and can be viewed:

1. **In GitHub:** Diagrams render automatically in markdown files
2. **In VS Code:** Install "Markdown Preview Mermaid Support" extension
3. **Online:** Copy Mermaid code to https://mermaid.live/
4. **In Documentation Tools:** Most modern documentation tools support Mermaid

## Key Architecture Concepts

### Layered Architecture
The system follows ABP Framework's layered architecture pattern:
- **Separation of Concerns:** Each layer has distinct responsibilities
- **Dependency Direction:** Upper layers depend on lower layers
- **Abstraction:** Interfaces define contracts between layers

### Policy Enforcement
- **Deterministic:** Same input always produces same output
- **Priority-Based:** Rules evaluated in priority order
- **Backend-Only:** No UI-only enforcement (security)
- **YAML Configuration:** Policies defined in YAML files

### Authorization Model
- **Two-Tier:** Permissions (who can do what) + Policies (what is allowed)
- **Menu Integration:** Permissions control menu visibility
- **API Protection:** [Authorize] attributes protect endpoints
- **Policy Gates:** Additional business rules enforced via policies

### Request Pipeline
1. **Middleware:** Cross-cutting concerns (logging, security, correlation)
2. **Authentication:** JWT token validation
3. **Authorization:** Permission checks
4. **Business Logic:** AppService execution
5. **Policy Enforcement:** Business rule validation
6. **Data Access:** Repository pattern with EF Core

## Related Documentation

- **Full Architecture:** `docs/ARCHITECTURE.md`
- **Development Guide:** `docs/DEVELOPER_GUIDE.md`
- **Deployment Guide:** `docs/DEPLOYMENT.md`
- **Policy Documentation:** See policy YAML files in `etc/policies/`

## Diagram Maintenance

When updating diagrams:
1. Keep Mermaid syntax valid (no spaces in node IDs)
2. Use camelCase or PascalCase for node names
3. Avoid special characters in labels (use quotes if needed)
4. Test diagrams in mermaid.live before committing
5. Update this summary if adding new diagrams

---

**Last Updated:** 2026-01-02
