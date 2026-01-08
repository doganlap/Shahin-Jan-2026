# Database Port Configuration - Explained

## Answer: YES, Database is Connected to 2 Ports Simultaneously

### Port Configuration

**Container `grc-db`:**
- **Internal Port**: `5432` (PostgreSQL default - inside container)
- **Host Port**: `5433` (exposed to host machine)
- **Mapping**: `0.0.0.0:5433->5432/tcp`

This means:
- **From host machine**: Connect to `localhost:5433` → maps to container port `5432`
- **From Docker network**: Connect to `db:5432` (service name) → container port `5432`

### Why 2 Ports?

This is **normal Docker behavior**:
- **Port 5432** = Container's internal PostgreSQL port
- **Port 5433** = Host's external access port (to avoid conflicts with local PostgreSQL)

**Example:**
```bash
# From host machine (external)
psql -h localhost -p 5433 -U postgres -d GrcMvcDb

# From inside container (internal Docker network)
psql -h db -p 5432 -U postgres -d GrcMvcDb
```

Both connect to the **same database** in the same container!

## Current Setup

**Container Status:**
- ✅ `grc-db`: Running on port 5433 (host) / 5432 (container)
- ✅ `grcmvc-db`: Separate container (from different compose file, not in use)
- ✅ Both containers on `grc-system_grc-network`

**Connection Strings:**
- `.env`: `Host=db;Port=5432` (for Docker network) ✅ Correct
- `appsettings.json`: `Host=localhost;Port=5433` (for host access)

## Network Status

**Containers on same network:**
- `grc-db`: 172.18.0.2/16
- `grc-system-grcmvc-1`: 172.18.0.3/16
- ✅ Network connectivity: OK

**Connection Test:**
```bash
# From app container to DB
docker exec grc-system-grcmvc-1 ping -c 1 db
# Should work if network is connected
```

## Current Issue

**Not a port problem** - The issue is:
- ❌ Password authentication failure (`28P01`)
- ✅ Ports are correctly configured
- ✅ Network is connected
- ⚠️ Password mismatch between connection string and database

**Solution:**
Verify password matches:
```bash
# Test password from container
docker exec grc-system-grcmvc-1 sh -c 'PGPASSWORD=postgres psql -h db -U postgres -d GrcMvcDb -c "SELECT 1;"'
```

## Summary

✅ **Yes, database is on 2 ports** (5433 host, 5432 container) - This is normal and correct!
✅ **Network connectivity**: Working
⚠️ **Issue**: Password authentication, not port configuration
