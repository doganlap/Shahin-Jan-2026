# Database Port Configuration Analysis

**Date**: 2026-01-07

## Current Database Containers

### Container 1: `grc-db`
- **Image**: `postgres:15-alpine`
- **Host Port**: `5433` (exposed)
- **Container Port**: `5432` (internal)
- **Port Mapping**: `0.0.0.0:5433->5432/tcp, [::]:5433->5432/tcp`
- **Status**: Up 16 hours
- **Databases**: `GrcMvcDb`, `GrcAuthDb`

### Container 2: `grcmvc-db`
- **Status**: Up 43 hours (healthy)
- **Port**: `5432/tcp` (NOT exposed to host - internal only)
- **Note**: This appears to be a different container, possibly from a different docker-compose setup

### Host Machine
- **Port 5432**: Local PostgreSQL (127.0.0.1:5432) - separate installation
- **Port 5433**: Docker container `grc-db` mapping

## Port Configuration Answer

**Yes, the database container (`grc-db`) is accessible via 2 ports:**

1. **Port 5433** (Host) → **Port 5432** (Container Internal)
   - From host machine: `localhost:5433`
   - From Docker network: `db:5432` (using service name)

2. **Same database, different access methods:**
   - External access (from host): Port `5433`
   - Internal access (from containers): Port `5432` (via service name `db`)

## Connection String Configuration

**For application running in Docker container:**
```
Host=db;Port=5432  (uses Docker network service name)
```

**For application running on host machine:**
```
Host=localhost;Port=5433  (uses host port mapping)
```

## Issue: Application Can't Connect

The application is trying to connect but getting authentication errors:
- Error: `28P01: password authentication failed for user "postgres"`

**Possible causes:**
1. Connection string uses `Host=db` but container isn't on same network
2. Password mismatch between connection string and actual database
3. The `.env` file password doesn't match the database password

## Solution

The connection string in `.env` shows:
```
CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432
```

This is correct for Docker network access. The issue is likely:
- Network connectivity (container can't reach `db` service name)
- Password mismatch

**Verify network:**
```bash
docker network inspect grc-system_grc-network
# Check if both grc-db and grc-system-grcmvc-1 are on the network
```

## Summary

✅ **Database is on 2 ports** (host 5433, container 5432) - This is normal Docker port mapping
⚠️ **Connection issue**: Authentication failure, not port configuration issue
