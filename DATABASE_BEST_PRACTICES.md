# Database & Container Management Best Practices

**Purpose**: Prevent database connection failures, container conflicts, and data loss

## 🎯 Core Principles

1. **Single Source of Truth** for configuration
2. **Immutable Infrastructure** - containers are disposable
3. **Health Checks** - automatic detection of failures
4. **Backup Before Changes** - always backup before migrations
5. **Documentation** - every database change must be documented

---

## 1. Container Management Best Practices

### ✅ Use Docker Compose for All Services

**DO:**
```yaml
# docker-compose.yml
services:
  db:
    image: postgres:15-alpine
    container_name: grc-db  # Explicit naming prevents conflicts
    ports:
      - "${DB_PORT:-5433}:5432"  # Use environment variable
    networks:
      - grc-network  # Explicit network assignment
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER:-postgres}"]
      interval: 10s
      timeout: 5s
      retries: 5
```

**DON'T:**
- ❌ Start containers manually with `docker run`
- ❌ Create containers outside of docker-compose
- ❌ Use multiple containers for the same database
- ❌ Hardcode port numbers

### ✅ Container Naming Convention

**Rule**: Use consistent, explicit container names
```yaml
container_name: grc-db  # Clear, unique, project-prefixed
```

**Benefits**:
- Easy identification
- Prevents naming conflicts
- Simplifies debugging

### ✅ Network Isolation

**Rule**: All services on the same Docker network
```yaml
networks:
  grc-network:
    driver: bridge
    name: grc-system_grc-network
```

**Benefits**:
- Services communicate via service names (`db`, not `localhost`)
- Isolation from host network
- Predictable DNS resolution

---

## 2. Connection String Management

### ✅ Single Source of Truth: `.env` File

**Structure:**
```bash
# .env file (DO NOT COMMIT TO GIT)
# Database Configuration
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=GrcMvcDb
DB_PORT=5433

# Connection Strings
# For Docker network (container-to-container)
CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=${DB_USER};Password=${DB_PASSWORD};Port=5432

# For host access (external tools)
EXTERNAL_CONNECTION_STRING=Host=localhost;Database=GrcMvcDb;Username=${DB_USER};Password=${DB_PASSWORD};Port=${DB_PORT}
```

**Rules**:
1. ✅ Store in `.env` file (add to `.gitignore`)
2. ✅ Use environment variables in docker-compose
3. ✅ Never hardcode passwords in code
4. ✅ Use different connection strings for different contexts:
   - Docker network: `Host=db;Port=5432`
   - Host access: `Host=localhost;Port=5433`

### ✅ Connection String Priority

**Order of precedence** (highest to lowest):
1. Environment variables (`CONNECTION_STRING`)
2. `appsettings.Development.json`
3. `appsettings.json`

**Implementation:**
```csharp
// Program.cs
var connectionString = configuration.GetConnectionString("Default") 
    ?? configuration["CONNECTION_STRING"]
    ?? throw new InvalidOperationException("Connection string not configured");
```

### ✅ Validation on Startup

**Add connection validation:**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" });
```

---

## 3. Database Lifecycle Management

### ✅ Migrations Best Practices

**DO:**
1. **Always backup before migrations**
2. **Test migrations on development first**
3. **Use transactions when possible**
4. **Version control all migrations**
5. **Document breaking changes**

**Implementation:**
```bash
# Migration workflow
# 1. Backup
docker exec grc-db pg_dump -U postgres GrcMvcDb > backup_$(date +%Y%m%d_%H%M%S).sql

# 2. Create migration
dotnet ef migrations add MigrationName -p src/GrcMvc.EntityFrameworkCore

# 3. Review migration SQL
dotnet ef migrations script -p src/GrcMvc.EntityFrameworkCore

# 4. Apply migration
dotnet ef database update -p src/GrcMvc.EntityFrameworkCore
```

### ✅ Seeding Strategy

**Rule**: Idempotent seeding (can run multiple times safely)

**Implementation:**
```csharp
// ApplicationInitializer.cs
public async Task InitializeAsync()
{
    try
    {
        // Check if already initialized
        var exists = await _dbContext.Users.AnyAsync();
        if (exists)
        {
            _logger.LogInformation("Database already initialized, skipping seed");
            return;
        }

        // Seed data
        await SeedUsersAsync();
        await SeedRolesAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to initialize database");
        throw; // Fail fast - don't silently ignore errors
    }
}
```

**Best Practices**:
- ✅ Check if data exists before seeding
- ✅ Use transactions for atomic operations
- ✅ Log all seed operations
- ✅ Never seed in production automatically (use API endpoint)

---

## 4. Password & Credential Management

### ✅ Secure Password Handling

**Development:**
```bash
# .env file (gitignored)
DB_PASSWORD=postgres  # Simple for dev, but still in .env
```

**Production:**
```bash
# Use secrets management
# Option 1: Docker secrets
docker secret create db_password ./secrets/db_password.txt

# Option 2: Environment variables (set by CI/CD)
export DB_PASSWORD=$(cat /run/secrets/db_password)

# Option 3: Key vault (Azure Key Vault, AWS Secrets Manager)
```

**Rules**:
1. ❌ Never commit passwords to git
2. ✅ Use `.env` file (gitignored)
3. ✅ Rotate passwords regularly
4. ✅ Use different passwords per environment
5. ✅ Store production passwords in secrets manager

### ✅ Password Reset Procedure

**Documented process:**
```bash
# 1. Reset password in database
docker exec grc-db psql -U postgres -c "ALTER USER postgres PASSWORD 'new_password';"

# 2. Update .env file
echo "DB_PASSWORD=new_password" >> .env

# 3. Restart containers
docker compose down
docker compose up -d

# 4. Verify connection
docker exec grc-db psql -U postgres -c "SELECT 1;"
```

---

## 5. Monitoring & Health Checks

### ✅ Implement Comprehensive Health Checks

**docker-compose.yml:**
```yaml
services:
  db:
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER:-postgres} -d ${DB_NAME:-GrcMvcDb}"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s

  grcmvc:
    depends_on:
      db:
        condition: service_healthy  # Wait for DB to be healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

**Application Health Check:**
```csharp
// Program.cs
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

### ✅ Automated Monitoring

**Create monitoring script:**
```bash
#!/bin/bash
# scripts/monitor-db.sh

# Check container status
if ! docker ps | grep -q grc-db; then
    echo "ERROR: grc-db container is not running"
    exit 1
fi

# Check database connectivity
if ! docker exec grc-db pg_isready -U postgres > /dev/null 2>&1; then
    echo "ERROR: Database is not ready"
    exit 1
fi

# Check application health
if ! curl -f http://localhost:8888/health > /dev/null 2>&1; then
    echo "ERROR: Application health check failed"
    exit 1
fi

echo "All checks passed"
```

---

## 6. Backup & Recovery Strategy

### ✅ Automated Backups

**Create backup script:**
```bash
#!/bin/bash
# scripts/backup-db.sh

BACKUP_DIR="./backups"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p "$BACKUP_DIR"

# Backup GrcMvcDb
docker exec grc-db pg_dump -U postgres GrcMvcDb | gzip > "$BACKUP_DIR/grcmvcdb_$DATE.sql.gz"

# Backup GrcAuthDb
docker exec grc-db pg_dump -U postgres GrcAuthDb | gzip > "$BACKUP_DIR/grcauthdb_$DATE.sql.gz"

# Keep only last 30 days
find "$BACKUP_DIR" -name "*.sql.gz" -mtime +30 -delete

echo "Backup completed: $DATE"
```

**Schedule with cron:**
```bash
# Add to crontab: Daily backup at 2 AM
0 2 * * * /path/to/grc-system/scripts/backup-db.sh >> /var/log/grc-backup.log 2>&1
```

### ✅ Recovery Procedure

**Document recovery steps:**
```bash
# 1. Stop application
docker compose stop grcmvc

# 2. Restore database
gunzip < backups/grcmvcdb_20250107_020000.sql.gz | \
    docker exec -i grc-db psql -U postgres -d GrcMvcDb

# 3. Verify data
docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT COUNT(*) FROM \"Users\";"

# 4. Restart application
docker compose start grcmvc
```

---

## 7. Container Conflict Prevention

### ✅ Pre-Startup Checks

**Create startup script:**
```bash
#!/bin/bash
# scripts/start-safe.sh

# Check if containers already exist
if docker ps -a | grep -q "grc-db"; then
    echo "WARNING: grc-db container already exists"
    read -p "Remove existing container? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker compose down
    else
        echo "Aborted"
        exit 1
    fi
fi

# Check port availability
if netstat -tuln | grep -q ":5433 "; then
    echo "WARNING: Port 5433 is already in use"
    exit 1
fi

# Start services
docker compose up -d

# Wait for health checks
echo "Waiting for services to be healthy..."
sleep 10

# Verify
./scripts/monitor-db.sh
```

### ✅ Cleanup Procedures

**Regular cleanup:**
```bash
# Remove stopped containers
docker container prune -f

# Remove unused networks
docker network prune -f

# Remove unused volumes (CAREFUL - this deletes data)
# docker volume prune -f  # Only if you're sure
```

---

## 8. Documentation Requirements

### ✅ Required Documentation

**Every database change must include:**
1. **Migration Script** - SQL or EF migration
2. **Rollback Script** - How to undo the change
3. **Test Cases** - How to verify the change
4. **Impact Analysis** - What breaks if this fails
5. **Backup Verification** - Confirmation backup exists

**Template:**
```markdown
## Database Change: [Description]

**Date**: YYYY-MM-DD
**Author**: Name
**Type**: Migration / Seed / Schema Change

### Change Details
- What changed
- Why it changed
- Breaking changes

### Backup
- Backup location: `backups/db_YYYYMMDD_HHMMSS.sql.gz`
- Verified: [Yes/No]

### Rollback
1. Stop application
2. Restore backup: `[commands]`
3. Restart application

### Verification
- Test query: `[SQL]`
- Expected result: `[description]`
```

---

## 9. Environment-Specific Configuration

### ✅ Separate Configurations

**Structure:**
```
.env                    # Development (gitignored)
.env.example            # Template (committed)
.env.production         # Production (gitignored, managed by CI/CD)
appsettings.json        # Default
appsettings.Development.json  # Development overrides
appsettings.Production.json   # Production overrides
```

**Rules**:
- ✅ Never commit `.env` files
- ✅ Always commit `.env.example` with placeholder values
- ✅ Use environment variables for secrets
- ✅ Use `appsettings.json` for non-sensitive defaults

---

## 10. Error Handling & Logging

### ✅ Connection Error Handling

**Graceful failure:**
```csharp
// Program.cs
builder.Services.AddDbContext<GrcDbContext>(options =>
{
    try
    {
        var connectionString = configuration.GetConnectionString("Default");
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });
    }
    catch (Exception ex)
    {
        _logger.LogCritical(ex, "Failed to configure database connection");
        throw; // Fail fast - don't start with broken DB
    }
});
```

### ✅ Comprehensive Logging

**Log levels:**
- `Critical`: Connection failures, data loss risks
- `Error`: Database errors, migration failures
- `Warning`: Connection retries, slow queries
- `Information`: Successful connections, migrations
- `Debug`: Query details (only in development)

---

## 🚨 Emergency Response Checklist

### When Database Connection Fails:

1. **Check container status**
   ```bash
   docker ps | grep grc-db
   ```

2. **Check database logs**
   ```bash
   docker logs grc-db --tail 50
   ```

3. **Test connection manually**
   ```bash
   docker exec grc-db psql -U postgres -c "SELECT 1;"
   ```

4. **Verify network connectivity**
   ```bash
   docker network inspect grc-system_grc-network
   ```

5. **Check connection string**
   ```bash
   docker exec grc-system-grcmvc-1 env | grep CONNECTION
   ```

6. **Verify password**
   ```bash
   # Test with password from .env
   docker exec grc-db psql -U postgres -c "SELECT 1;"
   ```

7. **Check port conflicts**
   ```bash
   netstat -tuln | grep 5433
   ```

8. **Review recent changes**
   - Check git log for recent commits
   - Review .env changes
   - Check docker-compose.yml changes

---

## 📋 Quick Reference

### Daily Operations
```bash
# Start services
docker compose up -d

# Check status
docker compose ps

# View logs
docker compose logs -f db

# Backup
./scripts/backup-db.sh

# Health check
curl http://localhost:8888/health
```

### Weekly Maintenance
```bash
# Cleanup old backups
find ./backups -name "*.sql.gz" -mtime +30 -delete

# Update dependencies
docker compose pull

# Review logs for errors
docker compose logs --since 7d | grep -i error
```

### Monthly Review
- Review backup retention policy
- Audit user access
- Review connection string security
- Update documentation

---

## ✅ Implementation Checklist

- [ ] Create `.env.example` template
- [ ] Add `.env` to `.gitignore`
- [ ] Implement health checks
- [ ] Create backup script
- [ ] Create monitoring script
- [ ] Document connection string management
- [ ] Set up automated backups
- [ ] Create emergency response procedure
- [ ] Add connection retry logic
- [ ] Implement comprehensive logging
- [ ] Create container startup safety checks
- [ ] Document all database schemas
- [ ] Set up password rotation schedule

---

## Summary

**Key Takeaways:**
1. ✅ Use Docker Compose for everything
2. ✅ Single source of truth (.env file)
3. ✅ Health checks and monitoring
4. ✅ Regular backups
5. ✅ Comprehensive documentation
6. ✅ Fail-fast error handling
7. ✅ Idempotent operations
8. ✅ Environment-specific configs

**Prevents:**
- ❌ Container conflicts
- ❌ Connection failures
- ❌ Data loss
- ❌ Configuration drift
- ❌ Silent failures
- ❌ Unrecoverable states
