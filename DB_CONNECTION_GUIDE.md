# Database Connection Guide

## Issue Identified

The verification script revealed:
- ✅ **Internal container connection works** - Database is healthy
- ❌ **Host connection fails** - Password authentication error
- 🔍 **Root cause**: Connection string mismatch between Docker network and host access

## Connection String Differences

### For Application Running in Docker Container
When the application runs inside Docker (via `docker-compose`), it should use:
```
Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432
```
- `Host=db` - Uses Docker network service name
- `Port=5432` - Internal container port

### For Application Running on Host Machine
When running directly on the host (e.g., `dotnet run`), it should use:
```
Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433
```
- `Host=localhost` - Localhost on host machine
- `Port=5433` - Host-mapped port (from docker-compose.yml: `5433:5432`)

## Current Configuration

**docker-compose.yml** sets:
```yaml
environment:
  - ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
```

**Your `.env` file** currently has:
```
CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=postgres;Password=***;Port=5432
```

This is **correct for Docker deployment** but **incorrect if running app directly**.

## Solution Options

### Option 1: Use Environment-Specific Connection Strings (Recommended)

**For Docker deployment** (keep `.env` as-is):
```
CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432
```

**For local development** (add to `appsettings.Development.json` or user secrets):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433"
  }
}
```

### Option 2: Dual Connection Strings in .env

Add both and use appropriate one based on environment:
```bash
# For Docker container communication
CONNECTION_STRING_DOCKER=Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432

# For host machine access
CONNECTION_STRING_HOST=Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433

# Default (Docker)
CONNECTION_STRING=${CONNECTION_STRING_DOCKER}
```

### Option 3: Fix Password Mismatch (If That's the Issue)

If the password in `.env` doesn't match the database:
1. Check what password is actually set in PostgreSQL:
   ```bash
   docker exec grc-db psql -U postgres -c "ALTER USER postgres PASSWORD 'newpassword';"
   ```
2. Update `.env`:
   ```
   DB_PASSWORD=newpassword
   CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=postgres;Password=newpassword;Port=5432
   ```
3. Restart services:
   ```bash
   docker-compose restart db grcmvc
   ```

## Verification Steps

1. **Test Docker network connection** (from inside container):
   ```bash
   docker exec grcmvc psql -h db -U postgres -d GrcMvcDb -c "SELECT 1;"
   ```

2. **Test host connection** (from your machine):
   ```bash
   PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -c "SELECT 1;"
   ```

3. **Run verification script**:
   ```bash
   ./scripts/verify-db-connection.sh
   ```

4. **Check application health**:
   ```bash
   curl http://localhost:8888/api/system/health
   ```

## Expected Results After Fix

- `/api/system/health` returns `200 OK` with `"status": "healthy"`
- All data-dependent endpoints can load data
- No more `28P01` (password authentication failed) errors
- EF Core migrations can run successfully

## Troubleshooting

### Error: "password authentication failed for user postgres"

**Possible causes**:
1. Password in `.env` doesn't match actual DB password
2. Connection string uses wrong credentials
3. Database user doesn't exist

**Fix**:
```bash
# Reset password inside container
docker exec grc-db psql -U postgres -c "ALTER USER postgres PASSWORD 'postgres';"

# Verify
docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT 1;"
```

### Error: "could not connect to server"

**Possible causes**:
1. Wrong host (`db` vs `localhost`)
2. Wrong port (`5432` vs `5433`)
3. Container not running

**Fix**:
```bash
# Check container status
docker ps --filter "name=grc-db"

# Start if stopped
docker-compose up -d db

# Check port mapping
docker ps --filter "name=grc-db" --format "{{.Ports}}"
```

## Next Steps

1. ✅ Verify `.env` `CONNECTION_STRING` format matches your deployment scenario
2. ✅ If running in Docker: use `Host=db;Port=5432`
3. ✅ If running locally: use `Host=localhost;Port=5433`
4. ✅ Test connection using verification script
5. ✅ Restart application after changes: `docker-compose restart grcmvc`
6. ✅ Verify health endpoint: `curl http://localhost:8888/api/system/health`
