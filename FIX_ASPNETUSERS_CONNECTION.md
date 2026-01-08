# Fix: AspNetUsers Table Not Found Error

**Error**: `PostgresException: 42P01: relation "AspNetUsers" does not exist`

## Problem

The application is trying to query `AspNetUsers` table but cannot find it. This happens when:
1. The connection string points to the wrong database (GrcMvcDb instead of GrcAuthDb)
2. The connection string uses `localhost:5433` which doesn't work inside Docker container

## Solution

The connection string for `GrcAuthDb` needs to use the Docker network service name when running in containers.

### Current Configuration (appsettings.json)
```json
"ConnectionStrings": {
  "GrcAuthDb": "Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5433"
}
```

### Required for Docker (via .env)
```bash
# In .env file (for Docker container access)
ConnectionStrings__GrcAuthDb=Host=db;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5432
```

## Fix Steps

1. **Update .env file** with correct connection string for Docker network
2. **Restart application** to pick up new connection string
3. **Verify connection** works

## Verification

```bash
# Check if tables exist in GrcAuthDb
docker exec grc-db psql -U postgres -d GrcAuthDb -c "\dt" | grep AspNet

# Check connection string being used
docker exec grc-system-grcmvc-1 env | grep GrcAuth

# Test query
docker exec grc-db psql -U postgres -d GrcAuthDb -c "SELECT COUNT(*) FROM \"AspNetUsers\";"
```
