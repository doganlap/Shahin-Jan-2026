# Platform Database Inventory - Complete

**Date**: 2026-01-07
**Status**: Complete inventory of all databases in the GRC platform

## Database Containers

### Container 1: `grc-db` ✅ (ACTIVE)
- **Image**: `postgres:15-alpine`
- **Host Port**: `5433` → Container Port `5432`
- **Network**: `grc-system_grc-network`
- **Status**: Running
- **Purpose**: Primary database container for GRC application

### Container 2: `grcmvc-db` ❌ (NOT FOUND)
- **Status**: Container does not exist (removed or stopped)
- **Note**: Previously existed but is no longer present

### Host Machine PostgreSQL
- **Version**: PostgreSQL 16.11 (Ubuntu)
- **Port**: `5432` (local installation)
- **Status**: Installed but connection requires authentication
- **Note**: Separate from Docker containers

## Databases by Container

### `grc-db` Container (Primary - 3 User Databases)

| Database Name | Owner | Size | Purpose |
|--------------|-------|------|---------|
| **GrcMvcDb** | postgres | 19 MB | Main application database (entities, workflows, etc.) |
| **GrcAuthDb** | postgres | 7.9 MB | Authentication & authorization (ASP.NET Identity) |
| **postgres** | postgres | 7.5 MB | Default PostgreSQL database |

**Total User Databases in `grc-db`**: **3 databases**

**System/Template Databases** (not counted):
- `template0` - PostgreSQL template database
- `template1` - PostgreSQL template database

### `grcmvc-db` Container
- **Status**: Container not found (removed/stopped)
- **Databases**: 0

### Host Machine PostgreSQL
- **Status**: Not accessible without credentials
- **Note**: Likely has default `postgres` database + any user-created databases

## Summary Statistics

| Category | Count |
|----------|-------|
| **Total Database Containers** | 1 (active) |
| **Total User Databases** | **3** |
| **Active Databases** | 2 (GrcMvcDb, GrcAuthDb) |
| **System Databases** | 1 (postgres) |

## Database Breakdown

### Application Databases (2)
1. **GrcMvcDb** (19 MB)
   - Main GRC application data
   - Entities: Risks, Controls, Policies, Audits, Assessments, etc.
   - Workflows, Evidence, Reports

2. **GrcAuthDb** (7.9 MB)
   - ASP.NET Identity tables
   - Users, Roles, Claims
   - Authentication tokens

### System Databases (1)
3. **postgres** (7.5 MB)
   - Default PostgreSQL database
   - System catalog

## Connection Information

**Application Connection (Docker Network):**
```
Host=db;Port=5432;Database=GrcMvcDb;Username=postgres
Host=db;Port=5432;Database=GrcAuthDb;Username=postgres
```

**External Connection (Host Machine):**
```
Host=localhost;Port=5433;Database=GrcMvcDb;Username=postgres
Host=localhost;Port=5433;Database=GrcAuthDb;Username=postgres
```

## Recommendations

✅ **Current Setup**: Clean and organized
- Single active container (`grc-db`)
- Clear separation: Application DB vs Auth DB
- Total size: ~35 MB (very manageable)

📋 **Next Steps**:
1. Verify host PostgreSQL is not needed (if not, can ignore it)
2. Ensure backups cover both `GrcMvcDb` and `GrcAuthDb`
3. Monitor database growth as application scales

## Final Answer

**Total Databases in Platform: 3**
- 2 Application Databases (GrcMvcDb, GrcAuthDb)
- 1 System Database (postgres)

**Total Database Containers: 1** (`grc-db`)
